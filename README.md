# Vendor Risk Scoring API

A robust, enterprise-grade RESTful API built with **.NET 8** designed to automate third-party risk management. The system evaluates and scores vendors across financial health, operational stability, and security compliance facilitating risk matrix.
* You can find detailed documentation [RiskManagement.pdf](RiskManagement.pdf), core logics, and API endpoints, examples.


<p align="center">
  <img src="Risk.drawio.svg" alt="Architecture Diagram" width="800"/>
</p>

---

## 🛠️ Tech Stack & Architecture

* **Backend Framework:** .NET 8 (ASP.NET Core Web API)
* **Database & ORM:** PostgreSQL & Entity Framework Core (Code-First Migration & Seeder)
* **Caching:** Redis (Distributed Caching)
* **Logging:** Serilog (JSON Sink) paired with the ELK Stack (Elasticsearch, Logstash, Kibana).
* **Testing:** xUnit, Moq (Unit Testing)
* **Containerization:** Docker & Docker Compose (Multi-container orchestration)

---

## 🚀 Setup & Installation Guide
This project is fully containerized. 

### Prerequisites:
Docker Desktop installed and running.

### Step-by-Step Execution:
1- Clone the repository and navigate to the root directory (where docker-compose.yml is located).

### Run the following command to build the images and start all services in detached mode:
2- docker-compose up -d --build

### Verify that all containers (vendor_risk_api, vendor_risk_postgres, vendor_risk_elasticsearch, vendor_risk_logstash, vendor_risk_kibana) are running:
3- docker ps

### Access the Swagger UI to interact with the API:
URL: http://localhost:5188/swagger

### API EndPoints:
* GET /api/vendor
* POST /api/vendor
* GET /api/vendor/{id}/risk
* GET /api/vendor/leaderboard

### Monitoring & Centralized Logging (ELK)
The API is integrated with Serilog writing structured JSON logs to src/VendorRiskScoring.API/logs/. Logstash automatically ingests these logs into Elasticsearch.

### To view real-time logs and system health:
Open the Kibana dashboard at: http://localhost:5601

Go to Management > Stack Management > Data Views.

Create a Data View with the index pattern: vendor-risk-logs-*.

Navigate to Analytics > Discover to analyze request lifecycles, execution times, and error traces.

## 🧪 Testing
The solution includes an independent unit test project (VendorRiskScoring.Tests) verifying business rules and risk engine accuracy via xUnit and Moq.

### To execute tests locally:
dotnet test
