@echo off

pushd "%~dp0"
REM TODO: build c module
python -m pip install -e .
popd