#!/usr/bin/python3
"""
usage: geo_mean.py [-h] -c [COLUMN ...] [--ignore-header] -o OUTPUT [csv_file ...]

Calculate the geo means for the multiple CSV files.

positional arguments:
  csv_file              The csv files

options:
  -h, --help            show this help message and exit
  -c [COLUMN ...], --column [COLUMN ...]
                        The target columns to calculate the geo mean, start from 0.
  --ignore-header       Ignore the header row when calculating the geo mean
  -o OUTPUT, --output OUTPUT
                        Output file path
"""
import sys
import doctest
import csv
import re

from pathlib import Path
from argparse import ArgumentParser

import numpy
from scipy.stats import gmean


def parse_args():
    parser = ArgumentParser(description='Calculate the geo means for the multiple CSV files. ')
    parser.add_argument('csv_file', type=Path, nargs="*", action="extend", default=[], help='The csv files')
    parser.add_argument('-c', '--column', type=int, required=True, nargs="*", action="extend", default=[],  help='The target columns to calculate the geo mean, start from 0.')
    parser.add_argument('--ignore-header', action='store_true', default=False, help='Ignore the header row when calculating the geo mean')
    parser.add_argument('-o', '--output', default=Path(r'.\am.csv'), required=True, type=Path, help='Output file path')

    return parser.parse_args()


def safe_float(value):
    try:
        return float(value)
    except (ValueError, TypeError):
        return 0.0
    

def is_validate_csv(file: Path, required_row: int, required_col: int) -> bool:
    with open(file) as f:
        reader = csv.reader(f)
        row_count = 0
        for row in reader:
            row_count += 1
            if row_count == 1 and len(row) != required_col:
                return False
        if row_count != required_row:
            return False
    return True
            
            
def get_row_column_count(file: Path) -> tuple[int, int]:
    with open(file) as f:
        reader = csv.reader(f)
        row_count = 0
        col_count = 0
        for row in reader:
            row_count += 1
            if row_count == 1:
                col_count = len(row)
    return row_count, col_count
     
     
def _prepare_first_csv(file, columns: list[int], is_ignore_header:  bool = False) -> list: 
    rows = [row for row in csv.reader(open(file))] 
    for i, row in enumerate(rows):
        if i == 0 and is_ignore_header:
            continue
        
        for col in columns:
            row[col] = [row[col]]
    return rows    


def _wash_value(value) -> (str, str):
    """Remove all non-numeric characters except dot and minus sign. return the numeric part and non-numeric part."""
    pattern = re.compile(r'[^0-9.-]')
    value = str(value)
    number_only = pattern.sub('', value)
    none_number = value.replace(number_only, '')
    return number_only, none_number


def _geo_mean(rows, columns: list[int], is_ignore_header:  bool = False) -> list:
    for r in range(len(rows)):
        if r == 0 and is_ignore_header:
            continue
        
        for c in columns:
            tail = ''
            values = rows[r][c]
            cleaned_values = []
            for value in values:
                if value:
                    value, tail = _wash_value(value)
                    value = safe_float(value)
                    if value != 0 and not numpy.isnan(value):
                        cleaned_values.append(value)

            geo_mean = gmean(cleaned_values)
            rows[r][c] = f"{geo_mean:.2f}{tail}" if (geo_mean != 0 and not numpy.isnan(geo_mean)) else ''
            
    return rows


def geo_mean(csv_files: list[Path], columns: list[int], is_ignore_header:  bool = False) -> list:
    """
    Calculate the geo mean for the given columns in the csv files.

    >>> import os
    >>> from tempfile import NamedTemporaryFile
    >>> tmp1 = NamedTemporaryFile(delete=False, mode='w+', newline='')
    >>> tmp2 = NamedTemporaryFile(delete=False, mode='w+', newline='')
    >>> tmp1.write("a,b,c\\n1,2,3\\n4,5,6\\n")
    ...
    >>> tmp1.close()
    >>> tmp2.write("a,b,c\\n1,8,3\\n4,11,6\\n")
    ...
    >>> tmp2.close()
    >>> geo_mean([Path(tmp1.name), Path(tmp2.name)], [1])
    [['a', 'b', 'c'], ['1', 5.0, '3'], ['4', 8.0, '6']]
    >>> os.remove(tmp1.name)
    >>> os.remove(tmp2.name)
    """
    if not csv_files:
        print("*** Warning: No csv files provided! ***")
        return []
    
    max_column = max(columns)
    row_count = 0
    col_count = 0
    illegal_files = []
    for i, file in enumerate(csv_files):
        if i == 0:
            row_count, col_count = get_row_column_count(file)
            if col_count <= max_column:
                print(f"*** Error: {file} has no enough columns! ***")
                return []
        else:
            if not is_validate_csv(file, row_count, col_count):
                print(f"*** Warning: {file} has different row/column count with the first file! The result will be ignored. ***")
                illegal_files.append(file)
                
    csv_files = [file for file in csv_files if file not in illegal_files]
    if not csv_files:
        print("*** Error: No valid csv files to process! ***")
        return []
    
    rows = _prepare_first_csv(csv_files[0], columns, is_ignore_header)
    for file in csv_files[1:]:
        with open(file) as f:
            reader = csv.reader(f)
            for r, row in enumerate(reader):
                if is_ignore_header and r == 0:
                    continue
                for col in columns:
                    rows[r][col].append(row[col])
        
    rows = _geo_mean(rows, columns, is_ignore_header)
    return rows


def main(args):
    if any(not file.exists() for file in args.csv_file):
        print("*** Error: Some csv files do not exist! ***")
        return
    
    rows = geo_mean(args.csv_file, args.column, args.ignore_header)
    with open(args.output, 'w+', newline='') as f:
        writer = csv.writer(f)
        for row in rows:
            writer.writerow(row)


if __name__ == "__main__":
    if len(sys.argv) == 1:
        print(f"*** Run doctest for {__file__}! ***")
        doctest.testmod(optionflags=doctest.ELLIPSIS |
                        doctest.IGNORE_EXCEPTION_DETAIL)
    else:
        main(parse_args())