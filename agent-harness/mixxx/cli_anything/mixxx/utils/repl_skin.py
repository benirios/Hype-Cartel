class ReplSkin:
    def __init__(self, name, version=None):
        self.name = name
        self.version = version

    def print_banner(self):
        print(f"{self.name} CLI — version {self.version}")
        print("Type 'library list' to list tracks, 'exit' to quit")

    def create_prompt_session(self):
        return None

    def get_input(self, session=None, project_name=None, modified=False):
        return input('> ')

    def help(self, commands_dict):
        for k, v in commands_dict.items():
            print(k, '-', v)

    def success(self, msg):
        print('\u2713', msg)

    def error(self, msg):
        print('\u2717', msg)

    def warning(self, msg):
        print('!', msg)

    def info(self, msg):
        print('-', msg)

    def status(self, key, value):
        print(f"{key}: {value}")

    def table(self, headers, rows):
        # Simple table printer
        print(' | '.join(headers))
        print('-' * 60)
        for r in rows:
            print(' | '.join(str(x) if x is not None else '' for x in r))

    def print_goodbye(self):
        print('Goodbye')
