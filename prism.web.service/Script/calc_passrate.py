#! /usr/bin/env python3
"""                 
usage: calc_pass_rate.py [-h] [--output OUTPUT] --column COLUMN [--pass-criteria PASS_CRITERIA]
                         [file ...]

Compare the csv files and generate the comparison report.

positional arguments:
  file                  The csv files to compare. Support glob syntax *, ?, etc.

options:
  -h, --help            show this help message and exit
  --output OUTPUT, -o OUTPUT
                        The output of the compare result csv file. Support save as csv or .xlsx,      
                        .xls, .html format.
  --column COLUMN, -c COLUMN
                        The columns to calculate the pass rate, start from 0.
  --pass-criteria PASS_CRITERIA, -p PASS_CRITERIA
                        The criteria to identify the pass result in the specified column. Default is  
                        'pass'. Support glob syntax *, ?, etc.
"""
import sys
import pandas as pd
import argparse
import doctest
import re

from pathlib import Path
from glob import glob
from typing import Optional


def parse_args(test_args: Optional[str] = None) -> argparse.ArgumentParser:
    p = argparse.ArgumentParser(
        description="Compare the csv files and generate the comparison report.")

    p.add_argument("file",
                   type=str,
                   nargs="*",
                   action="extend",
                   default=[],
                   help="The csv files to compare. Support glob syntax *, ?, etc.")
    p.add_argument("--output",
                   "-o",
                   type=Path,
                   required=False,
                   help="The output csv file to save pass rate result. Only support csv format.")
    p.add_argument("--column", 
                   "-c",
                   type=int,
                   required=True,
                   help="The columns to calculate the pass rate, start from 0.")
    p.add_argument("--unique",
                   "-u",
                   action="store_true",
                   help="Drop duplicate rows before calculation.")
    p.add_argument("--group-by-column",
                   "-g",
                   type=int,
                   required=False,
                   nargs="*",
                   action="extend",
                   default=[],
                   help="The column to group the pass rate calculation, start from 0.")
    p.add_argument("--pass-criteria",
                   "-p",
                   type=str,
                   default="pass",
                   help="The criteria to identify the pass result in the specified column. Default is 'pass'. Support regex syntax.")
    if test_args:
        return p.parse_args(test_args.split())
    else:
        return p.parse_args()
    
    
def _get_files(patterns: list[str]) -> list[Path]:
    files: list[Path] = []
    for pattern in patterns:
        matched_files = glob(pattern)
        files.extend([Path(f) for f in matched_files])
    return files

    
def main(args: argparse.Namespace):
    files = _get_files(args.file)
    result = pd.DataFrame()
    
    if args.output:
        if args.output.suffix.lower() != ".csv":
            print("Only support csv format for output file.")
            args.output = args.output.with_suffix(".csv")
        sys.stdout = open(args.output, 'w')
    
    if args.group_by_column:
        print("File, Group,Pass,Total,Pass Rate")
    else:
        print("File, Pass,Total,Pass Rate")
    for f in files:
        df = pd.read_csv(f)
        if args.unique:
            df = df.drop_duplicates()
        
        if args.group_by_column:
            group_names = df.columns[args.group_by_column].tolist()
            grouped = df.groupby(group_names)
            for group_values, group_df in grouped:
                if not isinstance(group_values, tuple):
                    group_values = (group_values,)
                total_count = len(group_df)
                pass_count = len(group_df[group_df.iloc[:, args.column].str.match(args.pass_criteria, flags=re.IGNORECASE, na=False)])
                pass_rate = (pass_count / total_count) * 100 if total_count > 0 else 0
                group_desc = "\n".join([f"{f.stem},{value},{pass_count},{total_count},{pass_rate:.2f}%" for name, value in zip(group_names, group_values)])
                print(group_desc)
        else:
            total_count = len(df)
            pass_count = len(df[df.iloc[:, args.column].str.match(args.pass_criteria, flags=re.IGNORECASE, na=False)])
            pass_rate = (pass_count / total_count) * 100 if total_count > 0 else 0
        
            print(f"{f.stem},{pass_count},{total_count},{pass_rate:.2f}%")
        

if __name__ == "__main__":
    if len(sys.argv) == 1:
        print(f"*** Run doctest for {__file__}! ***")
        doctest.testmod(optionflags=doctest.ELLIPSIS |
                        doctest.IGNORE_EXCEPTION_DETAIL)
    else:
        main(parse_args())
