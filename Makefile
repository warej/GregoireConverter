#This Makefile acts as a cheat-sheet, simplifying the process of building and testing the web application located in the web/grg2gabc directory. It provides targets for installing dependencies, building the application in both development and production modes, serving the application, linting the code, running tests, and cleaning up build artifacts.

APP_DIR := web/grg2gabc


.PHONY: help
help:
	@grep -E '^[a-zA-Z_-]+:.*?## .*$$' $(MAKEFILE_LIST) | awk 'BEGIN {FS = ":.*?## "}; {printf "\033[36m%-20s\033[0m %s\n", $$1, $$2}'

.PHONY: install
install:
	cd $(APP_DIR) && npm install --ignore-scripts

.PHONY: build
build:
	cd $(APP_DIR) && npm run build -- --configuration development

.PHONY: build-prod
build-prod:
	cd $(APP_DIR) && npm run build:prod

.PHONY: serve
serve:
	cd $(APP_DIR) && npm start

.PHONY: lint
lint:
	cd $(APP_DIR) && npm run lint

.PHONY: test
test:
	cd $(APP_DIR) && npm test -- --watch=false --browsers=ChromeHeadless

.PHONY: clean
clean:
	rm -rf $(APP_DIR)/dist $(APP_DIR)/.angular
