# Vendor Risk Scoring API

A robust, enterprise-grade RESTful API built with **.NET 8** designed to automate third-party risk management. The system evaluates and scores vendors across financial health, operational stability, and security compliance using an extensible **Rule Engine (Strategy Pattern)**.

---

## 📚 Table of Contents
1. [Tech Stack & Architecture](#-tech-stack--architecture)
2. [Datasets & Seed Data Structure](#-datasets--seed-data-structure)
3. [Setup & Installation Guide](#-setup--installation-guide)
4. [API Endpoints & Examples](#-api-endpoints--examples)
5. [Monitoring & Centralized Logging (ELK)](#-monitoring--centralized-logging-elk)
6. [Testing](#-testing)

---

## 🛠️ 1. Tech Stack & Architecture

* **Backend Framework:** .NET 8 (ASP.NET Core Web API)
* **Database & ORM:** PostgreSQL & Entity Framework Core (Code-First Migration & Seeder)
* **Design Patterns:** Strategy Pattern (for modular risk rules), Options Pattern (for configuration), Dependency Injection.
* **Logging:** Serilog (JSON Sink) paired with the ELK Stack (Elasticsearch, Logstash, Kibana).
* **Testing:** xUnit, Moq (Unit Testing)
* **Containerization:** Docker & Docker Compose (Multi-container orchestration)

---

🚀 3. Setup & Installation Guide
This project is fully containerized. 

Prerequisites:
Docker Desktop installed and running.

Step-by-Step Execution:
1- Clone the repository and navigate to the root directory (where docker-compose.yml is located).

Run the following command to build the images and start all services in detached mode:
2- docker-compose up -d --build

Verify that all containers (vendor_risk_api, vendor_risk_postgres, vendor_risk_elasticsearch, vendor_risk_logstash, vendor_risk_kibana) are running:
3- docker ps

Access the Swagger UI to interact with the API:
URL: http://localhost:5188/swagger

📡 4. API Endpoints & Examples
Evaluate Vendor Risk
Endpoint: GET /api/vendor/{id}/risk

Description: Fetches the vendor, runs the rule engine, applies weighted calculations, and returns an aggregate risk score with a human-readable explanation.

Example Request:
GET /api/vendor/10/risk

Example Response (200 OK):
{
  "riskScore": 0.06,
  "riskLevel": "Low",
  "reason": "Financial Health between 50 and 80. and All security and compliance checks passed. significantly impact the financial and security compliance risk levels, resulting in a Low overall risk score."
}

Get All Vendors
Endpoint: GET /api/vendor

Description: Retrieves the list of all registered vendors from PostgreSQL.

5. Monitoring & Centralized Logging (ELK)
The API is integrated with Serilog writing structured JSON logs to src/VendorRiskScoring.API/logs/. Logstash automatically ingests these logs into Elasticsearch.

To view real-time logs and system health:
Open the Kibana dashboard at: http://localhost:5601

Go to Management > Stack Management > Data Views.

Create a Data View with the index pattern: vendor-risk-logs-*.

Navigate to Analytics > Discover to analyze request lifecycles, execution times, and error traces.

🧪 6. Testing
The solution includes an independent unit test project (VendorRiskScoring.Tests) verifying business rules and risk engine accuracy via xUnit and Moq.

To execute tests locally:
dotnet test