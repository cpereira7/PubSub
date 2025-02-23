# Build once, use everywhere
build:
	    docker build -t pubsub-app:latest ../ -f ./Dockerfile

# Start all services
up:
	    docker compose up -d

start:
		docker compose up

# Stop everything
down:
	    docker compose down

# Rebuild and restart (when dependencies change)
rebuild: build down up

# Tail subscriber logs
logs-sub:
	    docker logs -f redis_subscriber