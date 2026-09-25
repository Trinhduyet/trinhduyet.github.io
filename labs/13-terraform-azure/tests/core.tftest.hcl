mock_provider "azurerm" {}

run "workload_shape" {
  command = plan

  variables {
    application     = "orders"
    environment     = "prod"
    location        = "southeastasia"
    vnet_cidr       = "10.40.0.0/16"
    app_subnet_cidr = "10.40.10.0/24"
    owner           = "commerce-platform"
  }

  assert {
    condition     = azurerm_resource_group.workload.name == "rg-orders-prod"
    error_message = "resource group naming contract changed"
  }

  assert {
    condition     = azurerm_resource_group.workload.tags["managed_by"] == "terraform"
    error_message = "managed_by tag must remain terraform"
  }

  assert {
    condition     = contains(azurerm_virtual_network.workload.address_space, "10.40.0.0/16")
    error_message = "VNet address space does not match the requested CIDR"
  }

  assert {
    condition     = contains(azurerm_subnet.app.address_prefixes, "10.40.10.0/24")
    error_message = "application subnet prefix changed"
  }
}
