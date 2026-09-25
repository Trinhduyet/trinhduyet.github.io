locals {
  name_prefix = format("%s-%s", var.application, var.environment)

  common_tags = {
    application = var.application
    environment = var.environment
    owner       = var.owner
    managed_by  = "terraform"
  }
}

resource "azurerm_resource_group" "workload" {
  name     = format("rg-%s", local.name_prefix)
  location = var.location
  tags     = local.common_tags
}

resource "azurerm_virtual_network" "workload" {
  name                = format("vnet-%s", local.name_prefix)
  location            = azurerm_resource_group.workload.location
  resource_group_name = azurerm_resource_group.workload.name
  address_space       = [var.vnet_cidr]
  tags                = local.common_tags
}

resource "azurerm_subnet" "app" {
  name                 = "snet-app"
  resource_group_name  = azurerm_resource_group.workload.name
  virtual_network_name = azurerm_virtual_network.workload.name
  address_prefixes     = [var.app_subnet_cidr]
}
