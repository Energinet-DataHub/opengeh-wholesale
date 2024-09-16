import os
import shutil

from dataclasses import dataclass
from pathlib import Path


@dataclass
class DBUtilsFileInfoFixture:
    """
    This class mocks the DBUtils FileInfo object
    """

    path: str
    name: str
    size: int
    modificationTime: int


class DBUtilsSecretsFixture:
    def __init__(self):
        self.secrets = self

    def get(self, scope, name):
        return os.environ.get(name)


class DBUtilsFixture:
    """
    This class is used for mocking the behaviour of DBUtils inside tests.
    """

    def __init__(self):
        self.fs = self
        self.secrets = DBUtilsSecretsFixture()

    def _clean_path(self, path: str):
        return path.replace("file:", "")

    def cp(self, src: str, dest: str, recurse: bool = False):
        copy_func = shutil.copytree if recurse else shutil.copy
        copy_func(self._clean_path(src), self._clean_path(dest))

    def ls(self, path: str):
        _paths = Path(self._clean_path(path)).glob("*")
        _objects = [
            DBUtilsFileInfoFixture(
                str(p.absolute()), p.name, p.stat().st_size, int(p.stat().st_mtime)
            )
            for p in _paths
        ]
        return _objects

    def mkdirs(self, path: str):
        Path(self._clean_path(path)).mkdir(parents=True, exist_ok=True)

    def mv(self, src: str, dest: str, recurse: bool = False):
        copy_func = shutil.copytree if recurse else shutil.copy
        shutil.move(
            self._clean_path(src), self._clean_path(dest), copy_function=copy_func
        )

    def put(self, path: str, content: str, overwrite: bool = False):
        _f = Path(self._clean_path(path))

        if _f.exists() and not overwrite:
            raise FileExistsError("File already exists")

        _f.write_text(content, encoding="utf-8")

    def rm(self, path: str, recurse: bool = False):
        deletion_func = shutil.rmtree if recurse else os.remove
        deletion_func(self._clean_path(path))
