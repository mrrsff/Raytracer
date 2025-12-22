import sys
import os
def get_json_files(directory):
    """Retrieve all JSON files in the specified directory."""
    return [f for f in os.listdir(directory) if f.endswith('.json')]

def run_raytracer(json_file, preview):
    """Run the raytracer program with the specified JSON file using normal CMD."""

    cmd = f'make run ARGS="{json_file} {preview}"'
    exit_code = os.system(cmd)
    return exit_code

class params:
    def __init__(self, args):
        json_file = args[1]
        preview = ''
        compileProgram = False
        for arg in args[2:]:
            if arg in ('--preview', '--p'):
                preview = arg
            elif arg in ('--compile', '--c'):
                compileProgram = True
        self.json_file = json_file
        self.preview = preview
        self.compileProgram = compileProgram

def main():
    if len(sys.argv) < 2:
        print("Usage: python3 batchTest.py <directory> [--preview] [--compile]")
        sys.exit(1)

    p = params(sys.argv)
    if p.compileProgram:
        os.system('make fast')
    
    os.system('make shader')
    json_file = p.json_file
    full_path = os.path.abspath(json_file)
    print(f"Processing {json_file}...")
    run_raytracer(full_path, p.preview)
    sys.exit(0)


if __name__ == "__main__":
    main()
