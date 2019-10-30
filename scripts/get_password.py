#!/usr/bin/env python3
import subprocess, re

ansi_escape = re.compile(r'\x1B[@-_][0-?]*[ -/]*[@-~]')

result = subprocess.run(
    ['./scripts/show_conn_string.sh'], stdout=subprocess.PIPE)

connstr = ansi_escape.sub('', result.stdout.decode('utf-8'))
string_items = dict(entry.split('=') for entry in connstr.split(';') if len(entry.strip()) != 0)

print(string_items['Password'])
