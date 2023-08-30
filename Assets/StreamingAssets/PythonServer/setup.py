from setuptools import setup, find_packages
from pathlib import Path

NAME = '3Drummers'
VERSION = '1.0.0'
DESCRIPTION = 'A drum transcription tool'
URL = ''
AUTHOR = 'Six Coders'

HERE = Path(__file__).parent

def load_requirements(name):
    required = [i.strip() for i in open(HERE / name)]
    required = [i for i in required if not i.startswith('#')]
    return required

REQUIRED = load_requirements('requirements.txt')
PACKAGES = find_packages()

setup(
    name = NAME,
    version = VERSION,
    description = DESCRIPTION,
    url = URL,
    author = AUTHOR,
    packages = ["demucs","onsets_frames_transcription","visual_midi"],
    install_requires = REQUIRED,
    include_package_data = True,
    entry_points = {
        'console_scripts': ['3Drummers=main.py'],
    },
)
