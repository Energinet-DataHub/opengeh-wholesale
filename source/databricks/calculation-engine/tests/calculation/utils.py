# Copyright 2020 Energinet DataHub A/S
#
# Licensed under the Apache License, Version 2.0 (the "License2");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
#     http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.

import asyncio
import time


# Code from https://stackoverflow.com/questions/45717433/stop-structured-streaming-query-gracefully
# Helper method to stop a streaming query
def stop_stream_query(query, wait_time):
    """Stop a running streaming query"""
    while query.isActive:
        msg = query.status["message"]
        data_avail = query.status["isDataAvailable"]
        trigger_active = query.status["isTriggerActive"]
        if not data_avail and not trigger_active and msg != "Initializing sources":
            print("Stopping query...")
            query.stop()
        time.sleep(0.5)

    # Okay wait for the stop to happen
    print("Awaiting termination...")
    query.awaitTermination(wait_time)


async def job_task(job):
    try:
        job.awaitTermination()
    except asyncio.CancelledError:
        stop_stream_query(job, 5000)


def streaming_job_asserter(
    pyspark_job, verification_function, timeout_seconds=20
) -> bool:
    task = asyncio.create_task(job_task(pyspark_job))
    start = time.monotonic()
    while True:
        if verification_function():
            task.cancel()
            return True
        elapsed_seconds = time.monotonic() - start
        if elapsed_seconds > timeout_seconds:
            print(
                f"Expectation of asynchrounous job couldn't be verified in {timeout_seconds} seconds."
            )
            break

    task.cancel()
    return False
