# MongoDB Experiments

Multiple basic operation experiments of a .Net Core API working with MongoDB, including multiple ways of partially updating documents.

Example of supported operations:

-   Create a document
-   Read documents
-   Replace a document
-   Delete a document
-   Partially update documents

# How to run?

Open the solution in Visual Studio and run the project (F5). **You will need Docker installed.**

# How to test?

You can use the tool of your choice to query the Web API endpoints or use Postman.

I deployed my Postman collection there: [https://documenter.getpostman.com/view/1175921/T17FCowi?version=latest](https://documenter.getpostman.com/view/1175921/T17FCowi?version=latest)

# How it works?

Mongo is installed as a container side-by-side with the project itself. See the `docker-compose.yml` and `.env` files for more info.

> Side note: don't publish your secrets to git as I did in this repo (the `.env` file).

# How to contribute?

If you want to contribute ideas, use cases, refactoring, implementations, or whatnot, please start by opening an issue.
