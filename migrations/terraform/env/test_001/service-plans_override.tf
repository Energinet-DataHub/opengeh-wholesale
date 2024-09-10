module "func_service_plan" {
  sku_name = "EP3"
}

module "message_retriever_service_plan" {
  sku_name = "EP3"
}

module "message_processor_service_plan" {
  sku_name = "EP3"
}

module "webapp_service_plan" {
  sku_name = "P1v3"
}
