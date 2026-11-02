# fcg-notifications-api

Serviço de notificações da plataforma FIAP Cloud Games (FCG). Ele só escuta eventos do RabbitMQ e "envia" e-mails. O envio é **simulado**: em vez de falar com um servidor de e-mail, cada mensagem é escrita no log do serviço.

Faz parte da Fase 2 do Tech Challenge, em que o monólito da Fase 1 foi dividido em quatro microsserviços. A visão geral da arquitetura, os fluxos de evento e as instruções para subir tudo juntos estão no [fcg-orchestration](https://github.com/PedroRCampelo/fcg-orchestration). Este README cobre só o que é específico deste serviço.

## O que o serviço faz

- Quando um usuário se cadastra, "envia" o e-mail de boas-vindas.
- Quando um pagamento é **aprovado**, "envia" o e-mail de confirmação de compra. Pagamento recusado não gera e-mail: o serviço só registra no log que não enviou.

Não tem banco de dados, não valida JWT e não tem Swagger. O único endpoint HTTP é o de saúde.

Cada e-mail aparece no log numa linha fácil de achar:

```
>>>>>>>> E-MAIL ENVIADO (BoasVindas) <<<<<<<< Para: ana@exemplo.com | Assunto: Bem-vindo(a) à FIAP Cloud Games, Ana! | Mensagem: ...
>>>>>>>> E-MAIL ENVIADO (ConfirmacaoDeCompra) <<<<<<<< Para: usuário 2ce2fea9-... | Assunto: Compra confirmada: Hades | Mensagem: ...
```

Na confirmação de compra o destinatário aparece como o id do usuário (`usuário <id>`), porque o `PaymentProcessedEvent` não carrega o e-mail.

Para ver os e-mails: `docker compose logs notifications-api` (compose), `kubectl logs -n fcg deploy/notifications-api` (Kubernetes) ou direto no terminal do `dotnet run`.

## Eventos

| Direção | Evento | Por onde trafega | Efeito |
|---|---|---|---|
| Consome | `UserCreatedEvent` (`UserId`, `Nome`, `Email`) | fila `notifications.user-created` | E-mail de boas-vindas |
| Consome | `PaymentProcessedEvent` (`OrderId`, `UserId`, `GameId`, `Titulo`, `Status`) | fila `notifications.payment-processed` | E-mail de confirmação se `Status` for `Approved`; nada se for `Rejected` |
| Publica | nenhum | | |

As duas filas são declaradas pelos serviços que publicam (Users e Payments) na subida deles. Por isso, um evento publicado enquanto o Notifications está fora do ar fica retido na fila e é entregue quando ele sobe, em vez de ser descartado.

**Retentativas.** Se o processamento falhar, o MassTransit tenta de novo 3 vezes, com 2 segundos de intervalo. Um evento inválido (por exemplo, `Status` desconhecido ou usuário vazio) não é repetido: vai direto para a fila `<fila>_error`.

**Sem deduplicação.** A entrega do RabbitMQ é "pelo menos uma vez", e este serviço não guarda o que já processou, então uma reentrega da mesma mensagem geraria um segundo e-mail. Para um e-mail simulado isso foi aceito; com um envio real seria preciso deduplicar.

## Endpoints

Rodando com `dotnet run`, o serviço escuta na porta 5104; dentro do container, na 8080.

| Método | Rota | Acesso | Descrição |
|---|---|---|---|
| GET | `/health` | Anônimo | Saúde completa, inclui a conexão com o RabbitMQ. Serve de readiness. |
| GET | `/health/live` | Anônimo | Só confirma que o processo responde. Serve de liveness. |

## Variáveis de ambiente

No .NET, `__` (dois sublinhados) separa os níveis da configuração: `RabbitMq__Host` é a chave `RabbitMq:Host` do `appsettings.json`. Este é o serviço mais simples: não tem connection string nem chave JWT.

| Variável | Descrição | Obrigatória | Padrão |
|---|---|---|---|
| `RabbitMq__Host` | Host do RabbitMQ | Não | `localhost` |
| `RabbitMq__Port` | Porta AMQP | Não | `5672` |
| `RabbitMq__VirtualHost` | Virtual host | Não | `/` |
| `RabbitMq__Username` | Usuário do RabbitMQ | Sim¹ | Só em Development: `fcg` |
| `RabbitMq__Password` | Senha do RabbitMQ | Sim¹ | Só em Development: `fcg_password` |
| `RabbitMq__Queues__NotificationsUserCreated` | Fila do `UserCreatedEvent`. Igual à declarada pelo Users | Não | `notifications.user-created` |
| `RabbitMq__Queues__NotificationsPaymentProcessed` | Fila do `PaymentProcessedEvent`. Igual à declarada pelo Payments | Não | `notifications.payment-processed` |
| `ASPNETCORE_ENVIRONMENT` | Em `Development` o serviço lê o `appsettings.Development.json` | Não | `Development` no `dotnet run`, `Production` na imagem |

¹ Obrigatória em qualquer ambiente que não seja o Development. Rodando com `dotnet run`, o `appsettings.Development.json` já preenche com as credenciais do RabbitMQ do compose.

Se faltar algo obrigatório, o serviço escreve o motivo no log e encerra com código de saída 1.

## Como rodar isolado

Pré-requisitos: .NET 8 SDK e Docker.

O Notifications só precisa do RabbitMQ. O jeito mais simples é subir a infraestrutura com o compose do orquestrador (o [fcg-orchestration](https://github.com/PedroRCampelo/fcg-orchestration) deve estar clonado ao lado deste repositório):

```bash
cd ../fcg-orchestration
cp .env.example .env              # só na primeira vez; preencha JWT__SIGNINGKEY (openssl rand -hex 32)
docker compose up -d rabbitmq postgres
cd ../fcg-notifications-api
```

O compose exige a `JWT__SIGNINGKEY` no `.env` mesmo para subir só o RabbitMQ, porque valida o arquivo inteiro. Este serviço, em si, não usa essa chave.

Depois, com o .NET:

```bash
dotnet run --project src/FCG.Notifications.Api
```

O serviço sobe em http://localhost:5104/health. Para vê-lo trabalhando sem os outros serviços, publique um evento pelo painel do RabbitMQ (http://localhost:15672, usuário `fcg`, senha `fcg_password`), na exchange `FCG.Contracts:UserCreatedEvent`, ou suba o Users e faça um cadastro.

Ou com a imagem Docker, sem precisar do SDK:

```bash
docker build -t fcg-notifications-api:local .
docker run --rm -p 5104:8080 \
  -e RabbitMq__Host=host.docker.internal \
  -e RabbitMq__Username=fcg -e RabbitMq__Password=fcg_password \
  fcg-notifications-api:local
```

`host.docker.internal` é o nome que o Docker Desktop dá à máquina hospedeira, onde o compose publicou o RabbitMQ (porta 5672).

Para rodar os testes (não precisam de broker):

```bash
dotnet test
```

## Estrutura do código

Por ser um serviço pequeno e sem banco, é um único projeto web mais o de testes, sem as camadas dos outros:

```
src/
  FCG.Notifications.Api
    Consumers/        UserCreatedConsumer e PaymentProcessedConsumer
    Notifications/    composição dos e-mails e o "envio" (escrita no log)
    Messaging/        configuração do MassTransit, filas e retentativas
    Contracts/        cópia dos contratos de evento
tests/
  FCG.Notifications.Tests   testes de composição dos e-mails e de consumo com o harness do MassTransit
k8s/                        Deployment, Service, ConfigMap e Secret deste serviço
Dockerfile                  build multi-stage, compila para arm64 e amd64
```

## Decisões que afetam este serviço

As justificativas completas estão na seção [Decisões de projeto](https://github.com/PedroRCampelo/fcg-orchestration#decisões-de-projeto) do fcg-orchestration. As que mais pesam aqui:

- **Contrato de evento duplicado:** a pasta `Contracts/` é uma cópia idêntica à dos repositórios que publicam esses eventos, sem pacote compartilhado. O `PaymentStatus` tem valores fixos (`Approved = 1`, `Rejected = 2`) e vai como texto no JSON.
- **Quem publica declara a fila de quem consome**, por isso nada se perde enquanto este serviço está fora do ar.
- **Nomes de fila repetidos** no `k8s/configmap.yaml`, iguais aos dos serviços que publicam.
- **Credenciais padrão nos manifestos do Kubernetes** (`k8s/secret.yaml`) servem só para o ambiente local.
