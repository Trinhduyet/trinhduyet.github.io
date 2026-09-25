output "resource_group_name" {
  description = "Name of the workload resource group."
  value       = azurerm_resource_group.workload.name
}

output "virtual_network_name" {
  description = "Name of the workload virtual network."
  value       = azurerm_virtual_network.workload.name
}

output "app_subnet_id" {
  description = "Resource ID of the application subnet."
  value       = azurerm_subnet.app.id
}
