from setuptools import setup, find_namespace_packages

setup(
    name="cli-anything-mixxx",
    version="0.1.0",
    packages=find_namespace_packages(include=["cli_anything.*"]),
    install_requires=[
        "click>=8.0.0",
        "prompt-toolkit>=3.0.0",
    ],
    entry_points={
        "console_scripts": [
            "cli-anything-mixxx=cli_anything.mixxx.mixxx_cli:main",
        ],
    },
    python_requires=">=3.8",
)
