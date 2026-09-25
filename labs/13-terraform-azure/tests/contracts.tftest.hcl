mock_provider "azurerm" {}

run "rejects_invalid_environment" {
  command = plan

  variables {
    application = "orders"
    environment = "production"
  }

  expect_failures = [
    var.environment,
  ]
}

run "rejects_invalid_application_name" {
  command = plan

  variables {
    application = "Orders_API"
    environment = "dev"
  }

  expect_failures = [
    var.application,
  ]
}
