# Developer FAQ

## How to update the Docker image used for Databricks development

The Docker image is being built and published as part of the CD. The Docker image is thus not used in the actual pull-request.

So in order to make changes to the code that requires changes to the Docker image as-well, it is necessary to split the changes into (at least) two pull-requests. The first updating the image. The latter updating the code. Please be aware that in the meantime betweeen the two commits the repo might somewhat be broken and might affect developers in their work on their own machines.
