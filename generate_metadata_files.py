import argparse
import os

import environment


def parse_arguments():
	parser = argparse.ArgumentParser()
	parser.add_argument("project", help = "set the project name")
	return parser.parse_args()


def write_product_information(version):
	information = {
		"assembly_version": version["numeric"],
		"assembly_file_version": version["numeric"],
		"assembly_informational_version": version["full"],
	}

	with open("ProductInformation.template.cs", "r") as template_file:
		file_content = template_file.read()
	file_content = file_content.format(**information)
	with open("ProductInformation.cs", "w") as product_file:
		product_file.write(file_content)


if __name__ == "__main__":
	base_directory = os.path.dirname(os.path.realpath(__file__))
	os.chdir(base_directory)

	arguments = parse_arguments()
	project = arguments.project
	project_version = environment.get_version()
	print("{project} version {project_version[full]}".format(**locals()))
	write_product_information(project_version)