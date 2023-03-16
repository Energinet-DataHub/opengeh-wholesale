locals {
    task_start_trigger = "start_${uuid()}"
    task_mp_start_trigger = "mp_${local.task_start_trigger}"
}