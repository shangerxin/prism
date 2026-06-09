# Prism
An management system for helping organize the test results, dashboard, test nodes, test suites etc.

## Setup Development Environment
1. Install Visual Studio 2026.
2. Install .NET 10 SDK.
3. Install MongoDB 8.0+.
4. Install Node.js 18+.
5. Install Git.
6. Install SQL Server Management Studio (SSMS) (optional).
7. Install XMind 8+ (optional).

## Run prism.web.service
1. Open `Prism.slnx` in Visual Studio.
2. Set `prism.web.service` as Startup Project.
3. Start debugging (`F5`).
4. Default local service URL is `https://localhost:44303/`.

## Web Service API and Swagger
The Web API prefix is `api/v1`.

- Swagger UI: `/swagger`
- Swagger JSON document: `/swagger/docs/v1`
- API base path: `/api/v1`

For a default local run, use:

- `https://localhost:44303/swagger`
- `https://localhost:44303/swagger/docs/v1`
- `https://localhost:44303/api/v1`

### TestResult API (upload-related)
These endpoints are used by `prism.client`:

- `POST /api/v1/TestResult/AddResult/`
- `POST /api/v1/TestResult/AddEnvirnoment/`
- `POST /api/v1/TestResult/AddParameter/`
- `POST /api/v1/TestResult/AddMetadata/`

## Testing
1. Launch `prism.web.service`.
2. Run unit tests in Visual Studio Test Explorer.
3. Test the Web API with Swagger UI or Postman.
4. You can also use test requests under `prism.model.Test/Fixtures/prism-test-requests.json`.
5. Use the `TestManagementDBEntities` connection string in `prism.web.service/Web.config` to switch the target database (for example, `initial catalog=TestManagementDBTest`).

## Use prism.client to Upload Test Result
`prism.client` is included as a git submodule. The Python client script is:

- `prism.client/prism.client.python/prism_client.py`

### 1) Install dependencies
From `prism.client/prism.client.python`:

```bash
pip install -r requirements.txt
```

### 2) Prepare a JSON result file
Example `result.json`:

```json
{
	"summary": {
		"throughput": 1234,
		"latency_ms": 21.4,
		"passed": true
	},
	"details": [
		{
			"name": "case_001",
			"status": "Pass",
			"duration_ms": 120
		}
	]
}
```

### 3) Upload result
Run from `prism.client/prism.client.python`:

```bash
python prism_client.py https://localhost:44303/api/v1 \
	-p YourProject \
	-t YourTestJob \
	-m result \
	-n benchmark-run-20260609 \
	-j result.json \
	-b Pass \
	-r Pass
```

Notes:
- `host` must include the API prefix, for example `https://localhost:44303/api/v1`.
- `-g` can be provided to set a specific build GUID. If omitted, one is auto-generated.
- Use `-m env`, `-m param`, or `-m meta` to upload environment/parameter/metadata with the same flow.