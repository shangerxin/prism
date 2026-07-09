#! /usr/bin/env python3
"""
usage: gen_compare_result.py [-h] [--output [OUTPUT ...]] [--compare [COMPARE ...]]
                             [--reference [REFERENCE ...]] [--file-suffix FILE_SUFFIX]
                             [--compare-suffix COMPARE_SUFFIX] [--reference-suffix REFERENCE_SUFFIX]
                             [--compare-column [COMPARE_COLUMN ...]] [--compare-method {ratio,value,subtract}]    
                             [--is-revert-compare] [--revert-compare-columns [REVERT_COMPARE_COLUMNS ...]]        
                             [--insert-file-name-column INSERT_FILE_NAME_COLUMN]
                             [--is-convert-compare-as-value-to-test-state] [-k [KEEP_COLUMN ...]]
                             [--key-column [KEY_COLUMN ...]] [--column-title [COLUMN_TITLE ...]]
                             [--threshold [THRESHOLD ...]] [--percentage-ratio-columns] [--unique]
                             [--filter-row-with-value [FILTER_ROW_WITH_VALUE ...]]
                             [--sort-by-column [SORT_BY_COLUMN ...]] [--file-target-must-exist] [--version]       
                             [file ...]

Compare the csv files and generate the comparison report.

positional arguments:
  file                  The csv files to compare. Support glob syntax *, ?, etc.

options:
  -h, --help            show this help message and exit
  --output [OUTPUT ...], -o [OUTPUT ...]
                        The output of the compare result csv file. Support save as csv or .xlsx, .xls, .html      
                        format.
  --compare [COMPARE ...], -c [COMPARE ...]
                        The comparison csv reports. The files will be merged into one file first. Support glob    
                        syntax *, ?, etc.
  --reference [REFERENCE ...], -r [REFERENCE ...]
                        The reference csv reports. The files will be merged into one file first. Support glob     
                        syntax *, ?, etc.
  --file-suffix FILE_SUFFIX
                        The suffix to add to the file columns in the output.
  --compare-suffix COMPARE_SUFFIX
                        The suffix to add to the compare columns in the output.
  --reference-suffix REFERENCE_SUFFIX
                        The suffix to add to the reference columns in the output.
  --compare-column [COMPARE_COLUMN ...], -v [COMPARE_COLUMN ...]
                        The target columns to compare, start from 0. If not set, none columns will be compared.   
                        If used option insert-file-name-column, the index will shift by 1 and the 0 point to the  
                        inserted file name column.
  --compare-method {ratio,value,subtract}, -m {ratio,value,subtract}
                        The method to use for comparison.
  --is-revert-compare, -x
                        Whether to revert the comparison ratio. By default will use the file column divide by     
                        the compare file column to calculate the ratio.
  --revert-compare-columns [REVERT_COMPARE_COLUMNS ...], -z [REVERT_COMPARE_COLUMNS ...]
                        The target columns to revert compare ratio, start from 0. Should match the compare        
                        columns index. If used option insert-file-name-column, the index will shift by 1 and the  
                        0 point to the inserted file name column.
  --insert-file-name-column INSERT_FILE_NAME_COLUMN, -f INSERT_FILE_NAME_COLUMN
                        A regex pattern to filter string out of the file name to insert as the first column for   
                        helping identify the merged csv contents. Default is '.*' to match all name. Add brace    
                        to capture specific part.
  --is-convert-compare-as-value-to-test-state, -s
                        Whether to convert the compare as value result to test state like same, changed, new,     
                        missing to no_change, new_pass, new_fail, changed.
  -k [KEEP_COLUMN ...], --keep-column [KEEP_COLUMN ...]
                        The columns to keep in the output when merge the csv files into one, start from 0. If     
                        used option insert-file-name-column, the index will shift by 1 and the 0 point to the     
                        inserted file name column. The columns will be inserted after the key columns.
  --key-column [KEY_COLUMN ...]
                        The key columns to sort and locate the comparison row from the csv files, multiple        
                        columns can combine together, start from 0. If used option insert-file-name-column, the   
                        index will shift by 1 and the 0 point to the inserted file name column.
  --column-title [COLUMN_TITLE ...]
                        The title of the compare output column. The count should be equal to the final columns.   
                        If not set, use the original column titles.
  --threshold [THRESHOLD ...], -t [THRESHOLD ...]
                        The threshold of the cell value. If the value lower than the threshold then it will be    
                        highlight. Default is 0.95
  --percentage-ratio-columns, -p
                        Whether the compare result columns are using percentage style.
  --unique, -u          Drop duplicate rows before calculation.
  --filter-row-with-value [FILTER_ROW_WITH_VALUE ...], -w [FILTER_ROW_WITH_VALUE ...]
                        Only keep the specified rows with the value before comparison. The format is
                        column_index:value, e.g., 2:pass to only keep the rows with 'pass' in column 2. Multiple  
                        criteria can be specified.
  --sort-by-column [SORT_BY_COLUMN ...], -y [SORT_BY_COLUMN ...]
                        The columns to sort the final result dataframe. The index is based on the final output    
                        table. Start from 0.
  --file-target-must-exist, -e
                        Whether to enforce the input target files must exist. If set and no files found, will     
                        exit the program immediately.
  --version             Show the version of the script and exit.
                                     
Examples:
python report\\gen_compare_result.py *.csv -o summary_timms.csv --insert-file-name-column _(.+)_xpu -k 1 3                        
python report\\gen_compare_result.py left*accuracy.csv -c right*accuracy.csv -m value --key-column 0 2 -v 4 5 -o compare_summary.csv --insert-file-name-column _(.+)_xpu -s

"""
import sys
import argparse
import doctest
import re

from enum import Enum, auto
from pathlib import Path
from datetime import date
from glob import glob
from typing import Optional, Union
from unittest import result

import pandas as pd


class CompareMethods(Enum):
    ratio    = auto()
    value    = auto()
    subtract = auto()
    

class CompareValueResults(Enum):
    same    = auto()
    changed = auto()
    new     = auto()
    missing = auto()


def week_of_year(targetDate: date = date.today()) -> int:
    week_number = targetDate.isocalendar()[1]
    return week_number


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
                   nargs="*",
                   action="extend",
                   default=[],
                   help="The output of the compare result csv file. Support save as csv or .xlsx, .xls, .html format.")
    p.add_argument("--compare",
                   "-c",
                   type=str,
                   nargs="*",
                   action="extend",
                   default=[], 
                   help="The comparison csv reports. The files will be merged into one file first. Support glob syntax *, ?, etc.")
    p.add_argument("--reference",
                   "-r",
                   type=str,
                   nargs="*",
                   action="extend",
                   default=[],
                   help="The reference csv reports. The files will be merged into one file first. Support glob syntax *, ?, etc.")
    p.add_argument("--file-suffix",
                   type=str,
                   required=False,
                   default="_cur",
                   help="The suffix to add to the file columns in the output.")
    p.add_argument("--compare-suffix",
                   type=str,
                   required=False,
                   default="_cmp",
                   help="The suffix to add to the compare columns in the output.")
    p.add_argument("--reference-suffix",
                   type=str,
                   required=False,
                   default="_ref",
                   help="The suffix to add to the reference columns in the output.")
    p.add_argument("--compare-column", 
                   "-v",
                   type=int,
                   nargs="*",
                   action="extend",
                   required=False,
                   default=[],
                   help="The target columns to compare, start from 0. If not set, none columns will be compared. If used option insert-file-name-column, the index will shift by 1 and the 0 point to the inserted file name column.")
    p.add_argument("--compare-method",
                   "-m",
                   type=str,
                   choices=["ratio", "value", "subtract"],
                   required=False,
                   default='value',
                   help="The method to use for comparison.")
    p.add_argument("--is-revert-compare",
                   "-x",
                   action="store_true",
                   required=False,
                   default=False,
                   help="Whether to revert the comparison ratio. By default will use the file column divide by the compare file column to calculate the ratio.")
    p.add_argument("--revert-compare-columns",
                   "-z",
                   type=int,
                   nargs="*",
                   action="extend",
                   required=False,
                   default=[],
                   help="The target columns to revert compare ratio, start from 0. Should match the compare columns index. If used option insert-file-name-column, the index will shift by 1 and the 0 point to the inserted file name column.")
    p.add_argument("--insert-file-name-column",
                   "-f",
                   type=str,
                   required=False,
                   default="",
                   help="A regex pattern to filter string out of the file name to insert as the first column for helping identify the merged csv contents. Default is '.*' to match all name. Add brace to capture specific part.")
    p.add_argument("--is-convert-compare-as-value-to-test-state",
                   "-s",
                   action="store_true",
                   required=False,
                   default=False,
                   help="Whether to convert the compare as value result to test state like same, changed, new, missing to no_change, new_pass, new_fail, changed.")
    p.add_argument("-k", "--keep-column", 
                   type=int,
                   nargs="*",
                   action="extend",
                   required=False,
                   default=[],
                   help="The columns to keep in the output when merge the csv files into one, start from 0. If used option insert-file-name-column, the index will shift by 1 and the 0 point to the inserted file name column. The columns will be inserted after the key columns.")
    p.add_argument("--key-column", 
                   type=int,
                   nargs="*",
                   required=False,
                   default=[],
                   action="extend",
                   help="The key columns to sort and locate the comparison row from the csv files, multiple columns can combine together, start from 0. If used option insert-file-name-column, the index will shift by 1 and the 0 point to the inserted file name column.")
    p.add_argument("--column-title", 
                   nargs="*",
                   action="extend",
                   required=False,
                   default=[],
                   type=str,
                   help="The title of the compare output column. The count should be equal to the final columns. If not set, use the original column titles.")
    p.add_argument("--threshold",
                   "-t",
                   nargs="*",
                   action="extend",
                   type=float,
                   required=False,
                   default=[],
                   help="The threshold of the cell value. If the value lower than the threshold then it will be highlight. Default is 0.95")
    p.add_argument("--percentage-ratio-columns",
                   "-p",
                   action="store_true",
                   required=False,
                   default=False,
                   help="Whether the compare result columns are using percentage style.")
    p.add_argument("--unique",
                   "-u",
                   default=False,
                   action="store_true",
                   help="Drop duplicate rows before calculation.")
    p.add_argument("--filter-row-with-value",
                   "-w",
                   nargs="*",
                   action="extend",
                   type=str,
                   required=False,
                   default=[],
                   help="Only keep the specified rows with the value before comparison. The format is column_index:value, e.g., 2:pass to only keep the rows with 'pass' in column 2. Multiple criteria can be specified.")
    p.add_argument("--sort-by-column",
                   "-y",
                   nargs="*",
                   action="extend",
                   type=int,
                   required=False,
                   default=[],
                   help="The columns to sort the final result dataframe. The index is based on the final output table. Start from 0.")
    p.add_argument("--file-target-must-exist",
                   "-e",
                   action="store_true",
                   required=False,
                   default=False,
                   help="Whether to enforce the input target files must exist. If set and no files found, will exit the program immediately.")
    p.add_argument("--version",
                   action="store_true",
                   help="Show the version of the script and exit.")
    if test_args:
        return p.parse_args(test_args.split())
    else:
        return p.parse_args()
    
    
def _get_files(patterns: list[str]) -> list[Path]:
    files: list[Path] = []
    for pattern in patterns:
        matched_files = glob(pattern)
        files.extend([Path(f) for f in matched_files if Path(f).is_file()])
    return files


def drop_duplicate_with_header_rows(df: pd.DataFrame) -> pd.DataFrame:
    if df.empty:
        return df
    
    dh = pd.DataFrame([df.columns])
    dh.columns = df.columns
    df = pd.concat([df, dh], ignore_index=True)
    df.drop_duplicates(inplace=True, keep='last')
    df = df.drop(df.tail(1).index)
    return df


def _concat(files: list[Path], insert_file_name_column: str = "", is_unique: bool=False) -> pd.DataFrame:
    if not files:
        return pd.DataFrame()
    
    df_list = []
    for file in files:
        df = pd.read_csv(file)
        if insert_file_name_column:
            file_name = file.name
            match = re.search(insert_file_name_column, file_name)
            if match:
                if match.groups():
                    df.insert(0, "file_name_info", match.group(1))
                else:
                    df.insert(0, "file_name_info", match.group(0))
                    
        df_list.append(df)
    merged_df = pd.concat(df_list, ignore_index=True)
    if is_unique:
        merged_df.drop_duplicates(inplace=True, keep='last')
        merged_df = drop_duplicate_with_header_rows(merged_df)
        merged_df.reset_index(drop=True, inplace=True)
            
    return merged_df


def _convert_merged_column_value(result_df, method, is_revert, column_name, left_suffix, right_suffix, is_convert_compare_as_value_to_test_state):
    result_df["_merge"] = result_df["_merge"].astype(str)
    if method == CompareMethods.ratio.name:
        if is_revert:
            result_df["_merge"] = result_df[f"{column_name}{right_suffix}"].astype(str).str.rstrip('%').astype(float) / result_df[f"{column_name}{left_suffix}"].astype(str).str.rstrip('%').astype(float)
        else:
            result_df["_merge"] = result_df[f"{column_name}{left_suffix}"].astype(str).str.rstrip('%').astype(float) / result_df[f"{column_name}{right_suffix}"].astype(str).str.rstrip('%').astype(float)
    elif method == CompareMethods.value.name:
        result_df.loc[(result_df["_merge"] == "both") & (result_df[f"{column_name}{left_suffix}"].isna()) & (result_df[f"{column_name}{right_suffix}"].isna()), "_merge"] = CompareValueResults.same.name
        result_df.loc[(result_df["_merge"] == "both") & (~result_df[f"{column_name}{left_suffix}"].isna()) & (result_df[f"{column_name}{left_suffix}"] != "") & (result_df[f"{column_name}{right_suffix}"].isna()), "_merge"] = CompareValueResults.new.name
        result_df.loc[result_df["_merge"] == "left_only", "_merge"] = CompareValueResults.new.name 
        result_df.loc[(result_df["_merge"] == "both") & (result_df[f"{column_name}{left_suffix}"].isna()) & (~result_df[f"{column_name}{right_suffix}"].isna()), "_merge"] = CompareValueResults.missing.name
        result_df.loc[result_df["_merge"] == "right_only", "_merge"] = CompareValueResults.missing.name 
        result_df.loc[result_df["_merge"] == "both", "_merge"] = CompareValueResults.changed.name
        result_df.loc[result_df.query(f"`{column_name}{left_suffix}` == `{column_name}{right_suffix}`").index, "_merge"] = CompareValueResults.same.name
    elif method == CompareMethods.subtract.name:
        if is_revert:
            result_df["_merge"] = result_df[f"{column_name}{right_suffix}"].astype(str).str.rstrip('%').astype(float) - result_df[f"{column_name}{left_suffix}"].astype(str).str.rstrip('%').astype(float)
        else:
            result_df["_merge"] = result_df[f"{column_name}{left_suffix}"].astype(str).str.rstrip('%').astype(float) - result_df[f"{column_name}{right_suffix}"].astype(str).str.rstrip('%').astype(float)
    else:
        raise ValueError(f"Unsupported compare method: {method}")
    
    if is_convert_compare_as_value_to_test_state and method == CompareMethods.value.name:
        changed_index = set(result_df.index[result_df["_merge"] == CompareValueResults.changed.name].to_list())
        new_index = set(result_df.index[result_df["_merge"] == CompareValueResults.new.name].to_list())
        pass_index = set(result_df.index[result_df[f"{column_name}{left_suffix}"].str.contains("pass", na=False)].to_list())
        left_not_pass_index = set(result_df.index.tolist()) - pass_index
        right_pass_index = set(result_df.index[result_df[f"{column_name}{right_suffix}"].str.contains("pass", na=False)].to_list())
        right_not_pass_index = set(result_df.index.tolist()) - right_pass_index
        failed_index = set(result_df.index[result_df[f"{column_name}{left_suffix}"].str.contains("fail", na=False)].to_list())
        new_pass_index = list((changed_index & pass_index) | (new_index & pass_index) | (pass_index & right_not_pass_index))
        new_fail_index = list((changed_index & failed_index) | (new_index & failed_index) | (left_not_pass_index & right_pass_index))
        new_pass_index.sort()
        new_fail_index.sort()
        result_df.loc[new_pass_index, "_merge"] = "new_pass"
        result_df.loc[new_fail_index, "_merge"] = "new_fail"


def compare(df1, df2, rdf, keep_columns: list[int], compare_columns: list[int], key_columns: list[int], method: str, is_revert: bool = False, revert_compare_columns: list[int] = [], file_suffix="",  compare_suffix="_cmp", reference_suffix="_ref", is_convert_compare_as_value_to_test_state: bool = False) -> pd.DataFrame:
    result_df = pd.DataFrame()    
    
    if df2.empty or df1.empty:
        return result_df
    
    if not key_columns:
        print('Warning: key_columns is not set the comparation wil be skipped.')
        return result_df
    
    key_column_names = [df1.columns[i] for i in key_columns]
    for column in compare_columns:
        is_column_revert = is_revert or (column in revert_compare_columns)
        mask_columns = key_columns.copy()
        mask_columns.append(column)
        mask_column_names = [df1.columns[i] for i in mask_columns]
        column_name = df1.columns[column]
        df1v = df1[mask_column_names]
        df2v = df2[mask_column_names]
        selected_columns = key_column_names + [f"{column_name}{file_suffix}", f"{column_name}{compare_suffix}", "_merge"]
        result_df[selected_columns] = df1v.merge(df2v, on=key_column_names, suffixes=(file_suffix, compare_suffix), how='outer', indicator=True)[selected_columns]
        _convert_merged_column_value(result_df, method, is_column_revert, column_name, file_suffix, compare_suffix, is_convert_compare_as_value_to_test_state)
        result_df.rename(columns={'_merge': f"{column_name}{file_suffix}/{column_name}{compare_suffix}"}, inplace=True)
    
        if not rdf.empty:
            selected_columns = key_column_names + [f"{column_name}{reference_suffix}", "_merge"]
            result_dfv = result_df[key_column_names + [f"{column_name}{file_suffix}"]]
            result_dfv.rename(columns={f"{column_name}{file_suffix}": f"{column_name}"}, inplace=True)
            rdfv = rdf[[rdf.columns[i] for i in key_columns + compare_columns]]
            merged = result_dfv.merge(rdfv, on=key_column_names, suffixes=(file_suffix, reference_suffix), how='outer', indicator=True)
            result_df = result_df.merge(merged[[f"{column_name}{reference_suffix}", "_merge"] + key_column_names], on=key_column_names, how='outer')
            _convert_merged_column_value(result_df, method, is_column_revert, column_name, file_suffix, reference_suffix, is_convert_compare_as_value_to_test_state)
            result_df.rename(columns={'_merge': f"{column_name}{file_suffix}/{column_name}{reference_suffix}"}, inplace=True)
            
    if rdf.empty:
        suffixes = [file_suffix, compare_suffix]
    else:
        suffixes = [file_suffix, compare_suffix, reference_suffix]
    reordered_columns = []
    for i in compare_columns:
        column_name = df1.columns[i]
        for suffix in suffixes:
            reordered_columns.append(f"{column_name}{suffix}")
        for suffix in suffixes[1:]:
            reordered_columns.append(f"{column_name}{file_suffix}/{column_name}{suffix}")
    result_df = result_df[key_column_names + reordered_columns]
    
    return result_df    
            
            
def _sort(df: pd.DataFrame, keep_columns: list[int], compare_columns: list[int], is_insert_file_name_column: Union[bool, str]):
    if df.empty:
        return
    
    sort_columns = set(keep_columns) - set(compare_columns)
    if is_insert_file_name_column:
        sort_columns.add(0)
        
    df.sort_values(by=[df.columns[i] for i in sort_columns], inplace=True)
    df.reset_index(drop=True, inplace=True)
    

def merge_keep_columns(result_df: pd.DataFrame, source: pd.DataFrame, keep_columns: list[int], key_columns: list[int], compare_columns: list[int]) -> pd.DataFrame:
    if source.empty or result_df.empty or not key_columns or not keep_columns:
        return result_df
    
    keep_columns = list(set(keep_columns) - set(compare_columns) - set(key_columns))
    keep_columns.sort()
    keep_column_names = [source.columns[i] for i in keep_columns]
    key_columns_names = [source.columns[i] for i in key_columns]
    source = source[key_columns_names + keep_column_names]
    result_df[keep_column_names] = result_df.merge(source, on=key_columns_names, how='outer', suffixes=('_left', '_right'), indicator=True)[keep_column_names]
    column_names = result_df.columns.tolist()
    column_names_ordered = key_columns_names + keep_column_names + [col for col in column_names if col not in key_columns_names + keep_column_names]
    result_df = result_df[column_names_ordered]
    return result_df


def decorate_result(result_df: pd.DataFrame, df1: pd.DataFrame, df2: pd.DataFrame, df3: pd.DataFrame, suffixes: set[str], thresholds: list[float], method: str, compare_columns: list[int], file_suffix="", compare_suffix="_cmp", reference_suffix="_ref") -> pd.DataFrame:
    highlight_cell_fills = ("FF7F7F", "FFD4D4", "FFE97F")
    thresholds.sort(reverse=True)
    if df2.empty or method == CompareMethods.value.name or not thresholds:
        return result_df
    
    for i in compare_columns:
        column_name = f"{df1.columns[i]}{file_suffix}"
        compare_column_name = f"{column_name}{compare_suffix}"
        reference_column_name = f"{column_name}{reference_suffix}"
        for i, threshold in enumerate(thresholds):
            if suffixes & set((".xlsx", ".xls")):
                # TODO: Need to verify the style apply works.
                result_df.style.map(lambda x: f"background-color: #{highlight_cell_fills[i % len(highlight_cell_fills)]};" if x < threshold else "", subset=[f"{column_name}/{compare_column_name}"])
                if not df3.empty:
                    for i, threshold in enumerate(thresholds):
                        result_df.style.applymap(lambda x: f"background-color: #{highlight_cell_fills[i % len(highlight_cell_fills)]};" if x < threshold else "", subset=[f"{column_name}/{reference_column_name}"])
             
            elif suffixes & set((".csv", ".html", ".htm")):
                result_df.loc[result_df[f"{column_name}/{compare_column_name}"] < threshold, f"{column_name}/{compare_column_name}"] = result_df.loc[result_df[f"{column_name}/{compare_column_name}"] < threshold, f"{column_name}/{compare_column_name}"].apply(lambda x: f"{x}{'↓' * (i+1)}")
                if not df3.empty:
                    result_df.loc[result_df[f"{column_name}/{reference_column_name}"] < threshold, f"{column_name}/{reference_column_name}"] = result_df.loc[result_df[f"{column_name}/{reference_column_name}"] < threshold, f"{column_name}/{reference_column_name}"].apply(lambda x: f"{x}{'↓' * (i+1)}")
    
    result_df.astype(str).fillna("", inplace=True)
    return result_df


def is_valid_float(x):
    try:
        x = float(x) 
        if str(x) in ("nan", "inf", "-inf"):
            return False
        else:
            return True
    except ValueError:
        return False
    

def convert_compare_results_to_percentage(result_df: pd.DataFrame, df1: pd.DataFrame, df2: pd.DataFrame, df3: pd.DataFrame, compare_columns: list[int],  file_suffix="", compare_suffix="_cmp", reference_suffix="_ref"):
    if result_df.empty:
        return result_df
    suffixes = []
    
    if not df2.empty and not df1.empty:
        suffixes.append(compare_suffix)
    if not df3.empty:
        suffixes.append(reference_suffix)
        
    if not suffixes:
        return result_df
        
    for i in compare_columns:
        column_name = df1.columns[i]
        for suffix in suffixes:
            compare_column_name = f"{column_name}{file_suffix}/{column_name}{suffix}"
            decrease_index = result_df.index[result_df[compare_column_name].astype(str).str.endswith('↓')].to_list()
            decrease_count = result_df.loc[decrease_index, compare_column_name].astype(str).str.count('↓')
            result_df.loc[decrease_index, compare_column_name] = result_df.loc[decrease_index, compare_column_name].astype(str).str.replace('↓', '', regex=False)
            result_df[compare_column_name] = result_df[compare_column_name].apply(lambda x: f"{x * 100:.2f}%" if is_valid_float(x) else x)
            result_df.loc[decrease_index, compare_column_name] = result_df.loc[decrease_index, compare_column_name].astype(str) + decrease_count.apply(lambda x: '↓' * x)
            
    return result_df


def make_compare_result_column_name_readable(result_df: pd.DataFrame, df1: pd.DataFrame, df2: pd.DataFrame, df3: pd.DataFrame, compare_columns: list[int], file_suffix="", compare_suffix="_cmp", reference_suffix="_ref") -> str:
    if result_df.empty:
        return result_df
    
    suffixes = []
    if not df2.empty and not df1.empty:
        suffixes.append(compare_suffix)
    if not df3.empty:
        suffixes.append(reference_suffix)
        
    if not suffixes:
        return result_df
        
    for i in compare_columns:
        column_name = df1.columns[i]
        for suffix in suffixes:
            compare_column_name = f"{column_name}{file_suffix}/{column_name}{suffix}"
            new_column_name = f"{column_name} ({file_suffix.lstrip('_')}/{suffix.lstrip('_')})"
            result_df.rename(columns={compare_column_name: new_column_name}, inplace=True)
    
    return result_df


def filter_rows_with_value(df: pd.DataFrame, filter_criteria: list[str]) -> pd.DataFrame:
    if df.empty or not filter_criteria:
        return df
    
    results = []
    for criteria in filter_criteria:
        try:
            column_index_str, value = criteria.split(":", 1)
            column_index = int(column_index_str)
            if column_index < 0 or column_index >= len(df.columns):
                raise ValueError(f"Column index {column_index} is out of range.")
            column_name = df.columns[column_index]
            results.append(df[df[column_name].astype(str) == value])
        except Exception as e:
            print(f"Warning: Failed to apply filter criteria '{criteria}': {e}")
    
    df = pd.concat(results)
    return df


def sort_final_result(df: pd.DataFrame, sort_by_columns: list[int]):
    if df.empty or not sort_by_columns:
        return
    
    df.sort_values(by=[df.columns[i] for i in sort_by_columns], inplace=True)
    df.reset_index(drop=True, inplace=True)


def unique_add(list1: list, list2: list) -> list:
    result = [x for x in list1]
    result = [x for x in list2 if x not in list1]
    return result

def remove_items(list1: list, list2: list) -> list:
    result = [x for x in list1 if x not in list2]
    return result

    
def main(args: argparse.Namespace):
    if args.version:
        print("gen_compare_result.py version 1.0.1")
        return
    
    files           = _get_files(args.file)
    compare_files   = _get_files(args.compare)
    reference_files = _get_files(args.reference)
    
    if args.file_target_must_exist and not files:
        print(f"Warning: No input target files match pattern {args.file}! Exiting...")
        return
    
    files_df     = _concat(files, args.insert_file_name_column, args.unique)
    compare_df   = _concat(compare_files, args.insert_file_name_column, args.unique)
    reference_df = _concat(reference_files, args.insert_file_name_column, args.unique)
    
    files_df     = filter_rows_with_value(files_df, args.filter_row_with_value)
    compare_df   = filter_rows_with_value(compare_df, args.filter_row_with_value)
    reference_df = filter_rows_with_value(reference_df, args.filter_row_with_value)
    
    args.compare_column = remove_items(args.compare_column, args.key_column)
    args.keep_column = remove_items(args.keep_column, args.key_column)
    key_columns = unique_add(args.keep_column, args.key_column)
    _sort(files_df, key_columns, args.compare_column, args.insert_file_name_column)
    _sort(compare_df, key_columns, args.compare_column, args.insert_file_name_column)
    _sort(reference_df, key_columns, args.compare_column, args.insert_file_name_column)
    
    if compare_df.empty or files_df.empty:
        result_df = files_df if not files_df.empty else compare_df
        if args.keep_column:
            result_df = result_df[[result_df.columns[i] for i in args.keep_column]]
    else:
        result_df = compare(files_df, compare_df, reference_df, args.keep_column, args.compare_column, args.key_column, args.compare_method, args.is_revert_compare, args.revert_compare_columns, args.file_suffix, args.compare_suffix, args.reference_suffix, args.is_convert_compare_as_value_to_test_state)
        result_df = merge_keep_columns(result_df, files_df, args.keep_column, args.key_column, args.compare_column)
        
    suffixes: set[str] = set((output.suffix.lower() for output in args.output))
    decorate_result(result_df, files_df, compare_df, reference_df, suffixes, args.threshold, args.compare_method, args.compare_column, args.file_suffix, args.compare_suffix, args.reference_suffix)
    convert_compare_results_to_percentage(result_df, files_df, compare_df, reference_df, args.compare_column, args.file_suffix, args.compare_suffix, args.reference_suffix)
    make_compare_result_column_name_readable(result_df, files_df, compare_df, reference_df, args.compare_column, args.file_suffix, args.compare_suffix, args.reference_suffix)
    sort_final_result(result_df, args.sort_by_column)
    
    if args.column_title:
        if len(args.column_title) != len(result_df.columns):
            raise ValueError("The count of column titles should be equal to the final columns.")
        result_df.columns = args.column_title
    
    if ".csv" in suffixes:
        output_paths = (output for output in args.output if output.suffix.lower() == ".csv")
        for output_path in output_paths:
            result_df.to_csv(output_path, index=False, encoding='utf-8-sig')
    if suffixes & set((".xlsx", ".xls")):
        output_paths = (output for output in args.output if output.suffix.lower() in (".xlsx", ".xls"))
        for output_path in output_paths:
            result_df.to_excel(output_path, index=False)
    if suffixes & set((".html", ".htm")):
        output_paths = (output for output in args.output if output.suffix.lower() in (".html", ".htm"))
        for output_path in output_paths:
            result_df.to_html(output_path, index=False, encoding='utf-8-sig', na_rep='')
            
    if not args.output:
        print(result_df.to_string(index=False))


if __name__ == "__main__":
    if len(sys.argv) == 1:
        print(f"*** Run doctest for {__file__}! ***")
        doctest.testmod(optionflags=doctest.ELLIPSIS |
                        doctest.IGNORE_EXCEPTION_DETAIL)
    else:
        main(parse_args())
