# Getting started with Databricks development in Wholesale

[![Code style: black](https://img.shields.io/badge/code%20style-black-000000.svg)](https://github.com/psf/black)

## Install necessary tools needed for development

- ### [Docker](https://www.docker.com/get-started)

    - use WSL 2, you will get a prompt with a guide after installing docker

- ### [Visual Studio Code](https://code.visualstudio.com/#alt-downloads) (system installer)

    - Extension called ***Remote - Containers***

## Get workspace ready for development

- Open ***opengeh-wholesale*** folder in Visual Studio Code

- Select ***Remote Explorer*** in the left toolbar

- Click on the ***plus icon*** in the top of the panel to the right of ***Containers*** and select ***Open Current Folder in Container***

- Wait for the container to build (*This will take a few minutes the first time*) and then you are ready to go

## Running Tests

- To run all test you will need to execute the following command in the workspace terminal

    ```text
    pytest
    ```

- For more verbose output use

    ```text
    pytest -vv -s
    ```

- To run tests in a specific folder simply navigate to the folder in the terminal and use the same command as above

- To run tests in a specific file navigate to the folder where the file is located in the terminal and execute the following command

    ```text
    pytest file-name.py
    ```

- You can also run a specific test in the file by executing the following command

    ```text
    pytest file-name.py::function-name
    ```

## Debugging Tests

- To debug tests you need to execute the following command

    Using debugz.sh with the following command

    ````text
    sh debugz.sh
    ````

    Or using command inside debugz.sh

    ```text
    python -m ptvsd --host 0.0.0.0 --port 3000 --wait -m pytest -v
    ```

- Create a ***launch.json*** file in the ***Run and Debug*** panel and add the following

    ```json
    {
        "name": "Python: Attach container",
        "type": "python",
        "request": "attach",
        "port": 3000,
        "host": "localhost"
    }
    ```

- Start debugging on the ***Python: Attach container*** in the ***Run and Debug*** panel
