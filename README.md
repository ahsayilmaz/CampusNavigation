# Campus Navigation System

A web application for finding the shortest path between buildings on a university campus. This application allows users to select starting and ending points via dropdown menus and calculates the optimal route, either based on the shortest distance or considering traffic conditions.

## Features

- Interactive map interface using Leaflet
- Building selection via dropdown menus
- Two routing options:
  - Shortest path by distance
  - Optimized path considering traffic conditions
- Visual path display on the map
- Step-by-step navigation instructions

## Tech Stack

- ASP.NET Core backend
- MySQL database for storing building and path data
- Leaflet.js for map visualization
- Dockerized deployment

## Running the Application

The application is containerized using Docker for easy deployment:

1. Ensure Docker and Docker Compose are installed on your system
2. Clone the repository
3. Run the application:

```bash
docker-compose up -d
```

The application will be available at http://localhost:5000

## Database Structure

The MySQL database contains the following tables:

- **Buildings**: Stores building information (name, coordinates)
- **BuildingConnections**: Stores the connections between buildings with distance and traffic information
- **UserLocations**: Tracks user locations (if enabled)
- **Users**: User account information

## Architecture

The application follows a clean architecture approach:

- **Controllers**: Handle API requests
- **Models**: Define data structures
- **Services**: Contain business logic
- **Data**: Database context and repositories

## Development

To run the application in development mode:

1. Install .NET Core SDK
2. Install MySQL
3. Update the connection string in `appsettings.json`
4. Run the application:

```bash
cd src/AzureSqlDockerApi
dotnet run
```

## Project Structure

- **Controllers**: Contains the API controllers that handle requests and responses.
  - `ApiController.cs`: Manages CRUD operations for the database.
  - `HomeController.cs`: Serves the main HTML page and static files.

- **Models**: Defines the data models used in the application.
  - `DatabaseModels.cs`: Represents the database entities.

- **Services**: Contains the business logic for interacting with the database.
  - `DatabaseService.cs`: Encapsulates methods for querying and manipulating data.

- **Data**: Manages the database context.
  - `DbContext.cs`: Configures the database connection and entity tracking.

- **wwwroot**: Contains static files served by the application.
  - `css/site.css`: Styles for the HTML page.
  - `js/site.js`: Client-side JavaScript functionality.
  - `index.html`: The main HTML file served by the application.

- **Configuration Files**:
  - `appsettings.json`: Configuration settings, including the Azure SQL connection string.
  - `appsettings.Development.json`: Development-specific settings.

- **Entry Point**:
  - `Program.cs`: Configures and starts the web host.

- **Project File**:
  - `AzureSqlDockerApi.csproj`: Specifies dependencies and project settings.

- **Tests**:
  - `ApiTests.cs`: Unit tests for the API controllers.

## Docker Setup

This project includes a Dockerfile and a docker-compose.yml file for building and running the application in a Docker container.

### Dockerfile

The Dockerfile contains instructions for setting up the environment and copying the necessary files for the application.

### docker-compose.yml

The docker-compose.yml file defines the services, networks, and volumes for the Docker application, allowing for easy orchestration of multiple containers.

## Getting Started

1. Clone the repository:
   ```
   git clone <repository-url>
   ```

2. Navigate to the project directory:
   ```
   cd AzureSqlDockerApi
   ```

3. Build the Docker image:
   ```
   docker-compose build
   ```

4. Run the application:
   ```
   docker-compose up
   ```

5. Access the application at `http://localhost:5000`.

## Contributing

Contributions are welcome! Please open an issue or submit a pull request for any enhancements or bug fixes.

## License

This project is licensed under the MIT License. See the LICENSE file for details.