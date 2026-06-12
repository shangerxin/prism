import sys
import os
import re
import argparse
import importlib
import importlib.util

from typing import Union, Iterable
from pathlib import Path


def to_path(path: Union[str, Path]) -> Path:
    if isinstance(path, str):
        return Path(path)
    elif isinstance(path, Path):
        return path
    else:
        raise ValueError(
            f"The parameter path {path} is required to be a str or a Path")


def isCaseInSensitiveEqual(src: str, value: str) -> bool:
    if src and value:
        return str(src).lower() == str(value).lower()
    else:
        return False


def isEqualModelName(src: str, value: str) -> bool:
    src = re.sub(r"_\d*S$", "", src)
    value = re.sub(r"_\d*S$", "", value)
    return isCaseInSensitiveEqual(src, value)


def isCaseInSensitiveContain(target: str, values: Iterable[str]) -> bool:
    if target and values:
        for value in values:
            if isCaseInSensitiveEqual(target, value):
                return True

    return False


def isCaseInSensitiveContainModelName(target: str, values: Iterable[str]) -> bool:
    if target and values:
        for value in values:
            if isEqualModelName(target, value):
                return True

    return False


def relative_load_module(relative_script_path: str, package: str = None):
    relative_script_path: Path = Path(relative_script_path)
    module_name   = relative_script_path.stem.replace('.', '_')
    loaded_module = importlib.util.resolve_name(module_name, package)
    if loaded_module not in sys.modules:
        current_dir        = os.path.dirname(os.path.abspath(__file__))
        module_script_path = Path(current_dir, relative_script_path)
        if module_script_path.exists():
            module = importlib.machinery.SourceFileLoader(module_name, str(module_script_path)).load_module()
            importlib.invalidate_caches()
            print(f"Loaded module from file {module_script_path}")
            return module
        else:
            raise Exception(f"Cannot find the module script file from {module_script_path}")


class ExtendAction(argparse.Action):
    """
    Add extend action for mitigate the gap between Python 3.6 doesn't support extend action

    Args:
        argparse (argparse.ArgumentParser): an instance of ArgumentParser
    """
    def __call__(self, parser, namespace, values, option_string=None):
        items = getattr(namespace, self.dest) or []
        items.extend(values)
        setattr(namespace, self.dest, items)


def to_float(value: str, default=0.0) -> float:
    """
    Convert string to float return default value when not success

    Args:
        value (str): The string represent a float value
        default (float, optional): The default value. Defaults to 0.0.

    Returns:
        float: The converted float value

    >>> to_float('0.5')
    0.5
    >>> to_float('abc')
    0.0
    >>> to_float('1e5')
    100000.0
    """
    try:
        return float(value)
    except:
        return default


def to_int(value: str, default=0) -> int:
    try:
        return int(value)
    except:
        return default