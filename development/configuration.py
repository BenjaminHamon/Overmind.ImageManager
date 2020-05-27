import datetime
import importlib
import os
import subprocess
import sys


def load_configuration(environment):
	configuration = {
		"project": "Overmind.ImageManager",
		"project_name": "Overmind Image Manager",
		"project_version": load_project_version(environment["git_executable"], "3.0"),
	}

	configuration["author"] = "Benjamin Hamon"
	configuration["author_email"] = "hamon.benjamin@gmail.com"
	configuration["organization"] = ""
	configuration["project_url"] = "https://github.com/BenjaminHamon/Overmind.ImageManager"
	configuration["copyright"] = "Copyright (c) 2020 Benjamin Hamon"

	configuration["development_toolkit"] = "git+https://github.com/BenjaminHamon/DevelopmentToolkit@{revision}#subdirectory=toolkit"
	configuration["development_toolkit_revision"] = "a4ad1edfb956641c420d42ff369087cffb6d7584"
	configuration["development_dependencies"] = [ "pylint" ]

	configuration["compilation_configurations"] = [ "debug", "release" ]

	configuration["project_identifier_for_artifact_server"] = "ImageManager"

	configuration["artifact_directory"] = "Artifacts"

	configuration["filesets"] = load_filesets(configuration["artifact_directory"])
	configuration["artifacts"] = load_artifacts(configuration["artifact_directory"])

	return configuration


def load_project_version(git_executable, identifier):
	branch = subprocess.check_output([ git_executable, "rev-parse", "--abbrev-ref", "HEAD" ], universal_newlines = True).strip()
	revision = subprocess.check_output([ git_executable, "rev-parse", "--short=10", "HEAD" ], universal_newlines = True).strip()
	revision_date = int(subprocess.check_output([ git_executable, "show", "--no-patch", "--format=%ct", revision ], universal_newlines = True).strip())
	revision_date = datetime.datetime.utcfromtimestamp(revision_date).replace(microsecond = 0).isoformat() + "Z"

	return {
		"identifier": identifier,
		"numeric": identifier,
		"full": identifier + "+" + revision,
		"branch": branch,
		"revision": revision,
		"date": revision_date,
	}


def load_filesets(artifact_directory):
	return {

		# Program binaries for development (executables, libraries, symbols and documentation)
		"binaries": {
			"path_in_workspace": os.path.join(artifact_directory, "{assembly}", "Binaries", "{configuration}"),
			"file_patterns": [ "{project}.{assembly}.exe", "{project}.{assembly}.exe.config", "*.dll", "*.pdb", "*.xml" ],
		},

		# Program binaries for release (executables and libraries)
		"binaries_stripped": {
			"path_in_workspace": os.path.join(artifact_directory, "{assembly}", "Binaries", "{configuration}"),
			"file_patterns": [ "{project}.{assembly}.exe", "{project}.{assembly}.exe.config", "*.dll" ],
		},

		# Additional resources, not embedded in programs
		"resources": {
			"path_in_workspace": ".",
			"file_patterns": [ "About.md", "License.txt" ],
		},

	}


def load_artifacts(artifact_directory):
	return {

		# Compilation output
		"binaries": {
			"file_name": "{project}.{assembly}_{version}_Binaries_{configuration}",
			"installation_directory": os.path.join(artifact_directory, "{assembly}", "Binaries", "{configuration}"),
			"path_in_repository": "Binaries",
			"filesets": [
				{ "identifier": "binaries", "path_in_archive": "." },
			],
		},

		# Development package
		"package": {
			"file_name": "{project}_{version}_Package_{configuration}",
			"path_in_repository": "Packages",
			"filesets": [
				{ "identifier": "binaries", "path_in_archive": ".", "parameters": { "assembly": "WallpaperService" } },
				{ "identifier": "binaries", "path_in_archive": ".", "parameters": { "assembly": "WindowsClient" } },
				{ "identifier": "resources", "path_in_archive": "." },
			],
		},

		# Release package
		"package_final": {
			"file_name": "{project}_{version}_PackageFinal",
			"path_in_repository": "Packages",
			"filesets": [
				{ "identifier": "binaries_stripped", "path_in_archive": ".", "parameters": { "assembly": "WallpaperService", "configuration": "Release" } },
				{ "identifier": "binaries_stripped", "path_in_archive": ".", "parameters": { "assembly": "WindowsClient", "configuration": "Release" } },
				{ "identifier": "resources", "path_in_archive": "." },
			],
		},

	}


def load_commands():
	all_modules = [
		"development.commands.artifact",
		"development.commands.clean",
		"development.commands.compile",
		"development.commands.develop",
		"development.commands.metadata",
		"development.commands.release",
		"development.commands.test",
	]

	return [ import_command(module) for module in all_modules ]


def import_command(module_name):
	try:
		return {
			"module_name": module_name,
			"module": importlib.import_module(module_name),
		}

	except ImportError:
		return {
			"module_name": module_name,
			"exception": sys.exc_info(),
		}
