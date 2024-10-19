# **EcommerceFullStackApp**

Welcome to the **EcommerceFullStackApp** repository!  
This project showcases a full-stack e-commerce solution built using **.NET Core Web APIs** for the backend and **Angular** for the frontend. The backend is designed as a collection of microservices, ensuring scalability, flexibility, and ease of development.

## **Project Overview**

### **Backend (Microservices)**

The backend is composed of the following independent microservices:

1. **CustomerDataApi**  
   Handles customer data management, including customer profiles, orders, and interactions.

2. **ProductsDataApiService**  
   Manages the e-commerce catalog, including product listings, details, pricing, and availability.

3. **SaleOrderDataService**  
   Manages order creation, order details, and customer purchase history.

4. **SaleOrderProcessingAPI**  
   Responsible for order processing and workflow. It communicates asynchronously with the Sales Invoice Generator using **RabbitMQ**.

5. **SalesInvoiceGeneratorAPIService**  
   Generates invoices for completed sales orders and sends them to customers via email.

### **Shared Code Library**

- **SalesAPILibrary**  
  Contains common code and utilities that are shared across the backend microservices, ensuring modularity and maintainability.

## **Microservices Communication**

The **SaleOrderProcessingAPI** and **SalesInvoiceGeneratorAPIService** microservices communicate via **RabbitMQ**, an asynchronous messaging system. This setup helps ensure efficient and decoupled communication between these critical services.

## **Frontend (Angular Application)**

The frontend is developed with **Angular**, providing a smooth and responsive user interface for customers to browse products, place orders, and view order history. It interacts with the backend services to manage all e-commerce operations.

## **Setup and Installation**

### **Clone the Repository**
git clone git@github.com:mohanrajmithun/EcommerceFullStackApp.git

cd EcommerceFullStackApp


### **Backend Setup**

Navigate to the Backend directory

cd Backend

### **Install dependencies for each service**

For each of the microservices, run the following commands:

cd <MicroserviceName>  # Example: cd CustomerDataApi

dotnet restore

### **Set up RabbitMQ**

Run RabbitMQ in a Docker container to enable inter-service communication:

docker run -d --hostname my-rabbit --name some-rabbit -p 5672:5672 -p 15672:15672 rabbitmq:3-management

### **Run Database Migrations**

Each microservice has a database migration setup. First, create a database called Ecommerce. Then, for each service, navigate to its directory and run the migration:

cd <MicroserviceName>  # Example: cd CustomerDataApi

dotnet ef database update

### **Repeat this step for**

CustomerDataApi

ProductsDataApiService

SaleOrderDataService

SaleOrderProcessingAPI

SalesInvoiceGeneratorAPIService

### **Run each microservice**

Open separate terminal windows for each microservice and run:

cd <MicroserviceName>  # Example: cd CustomerDataApi

dotnet run

Repeat for each service:

CustomerDataApi

ProductsDataApiService

SaleOrderDataService

SaleOrderProcessingAPI

SalesInvoiceGeneratorAPIService

### **Frontend Setup**

Navigate to the Frontend directory

cd ../Frontend

Install Angular dependencies

npm install

### **Run the Angular application**

ng serve

### **Access the Application**


Angular Application: http://localhost:4200
The user-facing frontend application.

### **Backend APIs: Each microservice will be running on its respective port (configured in each project).**

### **Key Features**

Microservices Architecture: Independent services for modularity and scalability.

RabbitMQ: Ensures efficient communication between key services.

Angular Frontend: Provides a dynamic, user-friendly interface for customers.

Shared Library: Common code to avoid duplication and ensure consistent functionality across services.

### **Contributing**
We welcome contributions! Whether it's bug fixes, new features, or suggestions for improvement, feel free to open an issue or submit a pull request.

### **License**
This project is licensed under the MIT License. See the LICENSE file for more details.

