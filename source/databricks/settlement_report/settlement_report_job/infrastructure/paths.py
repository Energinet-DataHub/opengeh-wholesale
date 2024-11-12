from settlement_report_job.entry_points.job_args.settlement_report_args import (
    SettlementReportArgs,
)


def get_settlement_reports_output_path(catalog_name: str) -> str:
    return f"/Volumes/{catalog_name}/wholesale_settlement_report_output/settlement_reports"  # noqa: E501


def get_report_output_path(args: SettlementReportArgs) -> str:
    return f"{args.settlement_reports_output_path}/{args.report_id}"
