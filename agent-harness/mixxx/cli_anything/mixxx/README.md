cli-anything-mixxx

Skeleton CLI harness for Mixxx.

Install (dev):
  cd agent-harness
  pip install -e .

Usage:
  cli-anything-mixxx library list --limit 20
  python -m cli_anything.mixxx.mixxx_cli  # REPL

Configuration:
  Set MIXXX_DB_PATH to point to your mixxxdb.sqlite if not using the default ~/.mixxx/mixxxdb.sqlite
