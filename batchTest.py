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


def main():
    if len(sys.argv) > 3:
        print("Usage: python3 batchTest.py <directory> --preview")
        sys.exit(1)

    target = sys.argv[1]
    preview = sys.argv[2] if len(sys.argv) == 3 and sys.argv[2] == '--preview' else ''

    os.system('make fast')  # Ensure the program is compiled
    # If single file instead of directory
    if not os.path.isdir(target):
        json_file = target
        full_path = os.path.abspath(json_file)
        print(f"Processing {json_file}...")
        run_raytracer(full_path, preview)
        sys.exit(0)

    json_files = get_json_files(target)
    if not json_files:
        print("No JSON files found in the specified directory.")
        sys.exit(0)

    for json_file in json_files:
        full_path = os.path.join(target, json_file)
        print(f"Processing {json_file}...")
        run_raytracer(full_path, preview)


if __name__ == "__main__":
    main()
