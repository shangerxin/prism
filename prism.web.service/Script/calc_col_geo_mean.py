#!/usr/bin/python3
"""
Calculate the geo means for the multiple CSV files. Append the 
result at the end of the csv and save into output root directory 
with the same name or as the output file.

positional arguments:
  csv_file              The csv files

options:
  -h, --help            show this help message and exit
  -c [COLUMN ...], --column [COLUMN ...]
                        The target columns to calculate the geo mean, start from 0.
  --ignore-header       Ignore the header row when calculating the geo mean
  -o OUTPUT, --output OUTPUT
                        Output root directory or a file path
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
    parser = ArgumentParser(description='Calculate the geo means for the multiple CSV files. Append the result at the end of the csv and save into output root directory with the same name or as the output file.')
    parser.add_argument('csv_file', type=Path, nargs="*", action="extend", default=[], help='The csv files')
    parser.add_argument('-c', '--column', type=int, required=True, nargs="*", action="extend", default=[],  help='The target columns to calculate the geo mean, start from 0.')
    parser.add_argument('-w', '--wash', action='store_true', default=False, help='Wash the values before calculating the new geo mean. Will remove all the non-numeric characters.')
    parser.add_argument('--ignore-header', action='store_true', default=False, help='Ignore the header row when calculating the geo mean')
    parser.add_argument('-o', '--output', default=Path('.'), required=True, type=Path, help='Output root directory or a file path')

    return parser.parse_args()


def safe_float(value):
    try:
        return float(value)
    except (ValueError, TypeError):
        return 0.0
    

def is_validate_csv(file: Path, required_col: int) -> bool:
    if not file.is_file():
        return False
    
    with open(file) as f:
        reader = csv.reader(f)
        for row in reader:
            if len(row) < required_col:
                return False
            else:
                return True
        return False
     
     
def _wash_value(value) -> (str, str):
    """Remove all non-numeric characters except dot and minus sign. return the numeric part and non-numeric part."""
    pattern = re.compile(r'[^0-9.-]')
    value = str(value)
    number_only = pattern.sub('', value)
    none_number = value.replace(number_only, '')
    return number_only, none_number

     
def _geo_mean(rows, columns: list[int], is_ignore_header:  bool = False, is_wash: bool = False) -> list:
    if is_ignore_header:
        start_row = 1
    else:
        start_row = 0
    
    column_count = len(rows[0])
    geo_mean_row = ['' for _ in range(column_count)] 
    for c in columns:
        values = []
        tail = ''
        for r in range(start_row, len(rows)):
            value = rows[r][c]
            if is_wash and value and str(value).strip():
                value, tail = _wash_value(value)
                 
            value = safe_float(value)
            if value:
                values.append(value)

        geo_mean = gmean(values)
        geo_mean_row[c] = f"{geo_mean:.2f}{tail}" if (geo_mean != 0 and not numpy.isnan(geo_mean)) else ''
        
    if any(geo_mean_row) and column_count > 1:
        geo_mean_row[0] = 'Geometry Mean'
            
    rows.append(geo_mean_row)
    return rows


def geo_mean(csv_files: list[Path], columns: list[int], is_ignore_header:  bool = False, is_wash: bool = False) -> list[list]:
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
    results = []
    for file in csv_files:
        if not is_validate_csv(file, max_column + 1):
            print(f"*** Error: The csv file {file} does not have enough columns or not exist! It will be ignored***")
            continue
        
        with open(file) as f:
            reader = csv.reader(f)
            rows = [row for row in reader]
            rows = _geo_mean(rows, columns, is_ignore_header, is_wash)
            results.append((file, rows))
            
    return results


def main(args):   
    results = geo_mean(args.csv_file, args.column, args.ignore_header, args.wash)
    
    if results:
        if args.output.is_dir():
            for file, rows in results:
                output_file = args.output / file.name
                with open(output_file, 'w+', newline='') as f:
                    writer = csv.writer(f)
                    for row in rows:
                        writer.writerow(row)
        else:
            with open(args.output, 'w+', newline='') as f:
                writer = csv.writer(f)
                for file, rows in results:
                    for row in rows:
                        writer.writerow(row)
                    
                    
if __name__ == "__main__":
    if len(sys.argv) == 1:
        print(f"*** Run doctest for {__file__}! ***")
        doctest.testmod(optionflags=doctest.ELLIPSIS |
                        doctest.IGNORE_EXCEPTION_DETAIL)
    else:
        main(parse_args())