'''
This file is used to setup the package of volumetric library.
'''
from setuptools import setup, find_packages

setup(
    name="volumetric",
    version="0.3.25",
    packages=find_packages(),
    include_package_data=True,
    description="Volumetric Python Library",
    author="Microsoft",
    author_email="volumetric@microsoft.com",
    url="https://aka.ms/volumetric",
    package_data={
        'volumetric.x64': ['*.pyd'],
    },
    install_requires=[],
    classifiers=[
        "Programming Language :: Python :: 3",
        "License :: OSI Approved :: MIT License",
        "Operating System :: Windows",
    ],)
