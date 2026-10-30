.PHONY: setup
setup:
	docker compose build

.PHONY: build
build:
	docker compose build configuration-api

.PHONY: serve
serve:
	docker compose build configuration-api && docker compose up configuration-api

.PHONY: shell
shell:
	docker compose run configuration-api bash

.PHONY: test
test:
	docker compose up localstack-configuration-api & docker compose build configuration-api-test && docker compose up configuration-api-test

.PHONY: lint
lint:
	-dotnet tool install -g dotnet-format
	dotnet tool update -g dotnet-format
	dotnet format

.PHONY: restart-db
restart-db:
	docker stop $$(docker ps -q --filter ancestor=test-database -a)
	-docker rm $$(docker ps -q --filter ancestor=test-database -a)
	docker rmi test-database
	docker compose up -d test-database
