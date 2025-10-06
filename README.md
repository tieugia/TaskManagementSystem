# Task Management System

A **Task Management System** web application for creating, updating, and managing tasks with search, filtering, and completion tracking.

---

## Requirements

- **.NET SDK 9.0** (required)  
  Download from: [https://dotnet.microsoft.com/download/dotnet/9.0](https://dotnet.microsoft.com/download/dotnet/9.0)

- **Visual Studio 2022**

---

## How to Run

1. Open the solution file **`TaskManagementSystem.sln`** in **Visual Studio 2022**.  
2. Restore NuGet packages if prompted.  
3. **Set as startup** the project you want to run:
   - **`TaskManagementSystem.Presentation`** → to open the **MVC web interface**.
   - **`TaskManagementSystem.API`** → to open the **API** and test it on **Swagger UI**.  
4. Press **F5** or click **Run ▶** to start the selected project.

---

## Additional notes

- Data is stored in an **in-memory database** and will **reset whenever the application restarts**.
