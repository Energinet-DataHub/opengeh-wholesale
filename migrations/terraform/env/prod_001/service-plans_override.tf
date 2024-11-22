module "func_service_plan" {
  sku_name                     = "EP3"
  maximum_elastic_worker_count = 1
}

module "message_retriever_service_plan" {
  sku_name                     = "EP3"
  maximum_elastic_worker_count = 1
}

module "message_processor_service_plan" {
  sku_name                     = "EP3"
  maximum_elastic_worker_count = 2
}

module "webapp_service_plan" {
  sku_name = "P1v3"
}
