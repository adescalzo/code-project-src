#!/bin/bash

# Color codes for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${GREEN}========================================${NC}"
echo -e "${GREEN}PostgreSQL with pgvector Setup Script${NC}"
echo -e "${GREEN}========================================${NC}"
echo ""

# Check if Docker is installed
if ! command -v docker &> /dev/null; then
    echo -e "${RED}Error: Docker is not installed.${NC}"
    echo "Please install Docker first: https://docs.docker.com/get-docker/"
    exit 1
fi

# Check if Docker Compose is installed
if ! command -v docker-compose &> /dev/null; then
    # Try docker compose (newer syntax)
    if ! docker compose version &> /dev/null; then
        echo -e "${RED}Error: Docker Compose is not installed.${NC}"
        echo "Please install Docker Compose: https://docs.docker.com/compose/install/"
        exit 1
    fi
    # Use newer syntax
    COMPOSE_CMD="docker compose"
else
    COMPOSE_CMD="docker-compose"
fi

# Function to wait for PostgreSQL to be ready
wait_for_postgres() {
    echo -e "${YELLOW}Waiting for PostgreSQL to be ready...${NC}"
    local max_attempts=30
    local attempt=1
    
    while [ $attempt -le $max_attempts ]; do
        if docker exec postgres-vector-db pg_isready -U postgres -d vectordb &> /dev/null; then
            echo -e "${GREEN}PostgreSQL is ready!${NC}"
            return 0
        fi
        echo -n "."
        sleep 2
        attempt=$((attempt + 1))
    done
    
    echo -e "${RED}PostgreSQL failed to start after $max_attempts attempts.${NC}"
    return 1
}

# Parse command line arguments
ACTION="up"

while [[ $# -gt 0 ]]; do
    case $1 in
        --down|--stop)
            ACTION="down"
            shift
            ;;
        --restart)
            ACTION="restart"
            shift
            ;;
        --logs)
            ACTION="logs"
            shift
            ;;
        --clean)
            ACTION="clean"
            shift
            ;;
        --help|-h)
            echo "Usage: $0 [OPTIONS]"
            echo ""
            echo "Options:"
            echo "  --down, --stop       Stop containers"
            echo "  --restart            Restart containers"
            echo "  --logs               Show container logs"
            echo "  --clean              Remove containers and volumes"
            echo "  --help, -h           Show this help message"
            echo ""
            echo "Examples:"
            echo "  $0                   Start PostgreSQL with pgvector"
            echo "  $0 --down            Stop container"
            echo "  $0 --clean           Clean up everything"
            exit 0
            ;;
        *)
            echo -e "${RED}Unknown option: $1${NC}"
            echo "Use --help for usage information"
            exit 1
            ;;
    esac
done

# Execute based on action
case $ACTION in
    up)
        echo -e "${YELLOW}Starting PostgreSQL with pgvector...${NC}"
        echo ""
        
        # Create .env file if it doesn't exist
        if [ ! -f .env ]; then
            echo -e "${YELLOW}Creating .env file from .env.example...${NC}"
            cp .env.example .env
            echo -e "${GREEN}.env file created. Please update it with your API keys.${NC}"
        fi
        
        # Start containers
        $COMPOSE_CMD up -d
        
        if [ $? -eq 0 ]; then
            echo -e "${GREEN}Container started successfully!${NC}"
            echo ""
            
            # Wait for PostgreSQL to be ready
            wait_for_postgres
            
            if [ $? -eq 0 ]; then
                echo ""
                echo -e "${GREEN}========================================${NC}"
                echo -e "${GREEN}Setup Complete!${NC}"
                echo -e "${GREEN}========================================${NC}"
                echo ""
                echo "PostgreSQL Connection:"
                echo -e "  Host:       ${GREEN}localhost${NC}"
                echo -e "  Port:       ${GREEN}5432${NC}"
                echo -e "  Database:   ${GREEN}vectordb${NC}"
                echo -e "  Username:   ${GREEN}postgres${NC}"
                echo -e "  Password:   ${GREEN}postgres${NC}"
                echo ""
                echo "Connection String:"
                echo -e "  ${GREEN}Host=localhost;Database=vectordb;Username=postgres;Password=postgres${NC}"
                echo ""
                echo "DBeaver Connection:"
                echo "  1. Create new PostgreSQL connection"
                echo "  2. Use the credentials above"
                echo "  3. Test connection"
                echo ""
                echo "Next steps:"
                echo "1. Update the .env file with your OpenAI API key"
                echo "2. Update appsettings.json if needed"
                echo "3. Run: dotnet restore"
                echo "4. Run: dotnet run"
            fi
        else
            echo -e "${RED}Failed to start container.${NC}"
            exit 1
        fi
        ;;
        
    down)
        echo -e "${YELLOW}Stopping container...${NC}"
        $COMPOSE_CMD down
        echo -e "${GREEN}Container stopped.${NC}"
        ;;
        
    restart)
        echo -e "${YELLOW}Restarting container...${NC}"
        $COMPOSE_CMD restart
        echo -e "${GREEN}Container restarted.${NC}"
        ;;
        
    logs)
        echo -e "${YELLOW}Showing container logs...${NC}"
        $COMPOSE_CMD logs -f
        ;;
        
    clean)
        echo -e "${YELLOW}WARNING: This will remove the container and volume!${NC}"
        read -p "Are you sure? (y/N) " -n 1 -r
        echo
        if [[ $REPLY =~ ^[Yy]$ ]]; then
            echo -e "${YELLOW}Cleaning up container and volume...${NC}"
            $COMPOSE_CMD down -v
            echo -e "${GREEN}Cleanup complete.${NC}"
        else
            echo -e "${YELLOW}Cleanup cancelled.${NC}"
        fi
        ;;
esac