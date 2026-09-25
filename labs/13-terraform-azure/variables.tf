variable "application" {
  type        = string
  description = "Short workload/application name."

  validation {
    condition     = can(regex("^[a-z][a-z0-9-]{1,20}$", var.application))
    error_message = "application must start with a lowercase letter and contain only lowercase letters, digits, or hyphens."
  }
}

variable "environment" {
  type        = string
  description = "Deployment environment."

  validation {
    condition     = contains(["dev", "staging", "prod"], var.environment)
    error_message = "environment must be dev, staging, or prod."
  }
}

variable "location" {
  type        = string
  description = "Azure region name."
  default     = "southeastasia"
}

variable "vnet_cidr" {
  type        = string
  description = "Address space for the workload VNet."
  default     = "10.20.0.0/16"
}

variable "app_subnet_cidr" {
  type        = string
  description = "Address prefix for the application subnet."
  default     = "10.20.1.0/24"
}

variable "owner" {
  type        = string
  description = "Operational owner used in tags."
  default     = "platform-learning"
}
