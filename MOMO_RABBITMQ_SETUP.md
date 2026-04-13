## RabbitMQ + Background Worker Integration - Docker Compose Setup Verification

### Services Configuration:
✅ **auth-api** (Port 5001)
- RabbitMQ Host: `rabbitmq` (resolves via Docker network)
- RabbitMQ Port: `5672`
- RabbitMQ User: `guest`
- RabbitMQ Password: `guest`
- Depends on: `rabbitmq` service with health check
- Database: Neon PostgreSQL

✅ **inventory-api** (Port 5002)
- No RabbitMQ dependency (only consumes via HTTP endpoint)
- Database: Same Neon PostgreSQL instance
- Endpoint: `POST /api/medicines/checkout/complete`

✅ **rabbitmq** (Port 5672, Management 15672)
- Default credentials: guest/guest
- Durable queue: `momo_payments`
- Health check enabled

### Flow Verification:

1. **Webhook Reception**
   - Momo IPN → `GET /api/momo/webhook?orderId=...&resultCode=0&signature=...`
   - Validates webhook signature
   - Publishes to RabbitMQ `momo_payments` queue

2. **Background Worker**
   - Polls every 1 second
   - Consumes messages from RabbitMQ
   - On success (ResultCode=0), calls:
     ```
     POST http://inventory-api:8080/api/medicines/checkout/complete
     Body: { "SaleId": "guid" }
     ```

3. **Inventory API**
   - Completes the sale (Pending → Completed)
   - Deducts from medicine batches
   - Generates receipt number

### Testing Steps:

1. **Verify RabbitMQ is running:**
   ```bash
   curl http://localhost:15672
   # Login: guest/guest
   ```

2. **Verify auth-api connects to RabbitMQ:**
   - Check Docker logs: `docker logs auth-api`
   - Should initialize without connection errors

3. **Test the flow:**
   - Create a pending sale via `/api/medicines/checkout`
   - Simulate Momo webhook with valid signature
   - Monitor RabbitMQ queue for message
   - Verify sale is marked as Completed in database

### Key Implementation Details:

**MomoPaymentMessage** - Minimal message model
**IMessageQueueService** - Publish/Consume interface
**RabbitMqMessageQueueService** - RabbitMQ implementation
**MomoPaymentWorkerService** - Background worker (BackgroundService)
**MomoController.Webhook** - IPN endpoint

All services use configuration from `appsettings.json` with Docker environment variable overrides.
