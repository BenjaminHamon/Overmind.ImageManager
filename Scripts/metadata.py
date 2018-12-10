import logging
import os


def configure_argument_parser(environment, configuration, subparsers):
	return subparsers.add_parser("metadata", help = "generate the project metadata files")


def run(environment, configuration, arguments):
	write_product_information(configuration["project_version"], arguments.simulate)


def write_product_information(version, simulate):
	information = {
		"assembly_version": version["numeric"],
		"assembly_file_version": version["numeric"],
		"assembly_informational_version": version["full"],
	}

	output_directory = os.path.join(".build", "Metadata")
	if not simulate and not os.path.isdir(output_directory):
		os.makedirs(output_directory)

	logging.info("Writing ProductInformation.cs")
	with open(os.path.join("Metadata", "ProductInformation.template.cs"), "r") as template_file:
		file_content = template_file.read()
	file_content = file_content.format(**information)
	if not simulate:
		with open(os.path.join(output_directory, "ProductInformation.cs"), "w") as product_file:
			product_file.write(file_content)
