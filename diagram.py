from diagrams import Diagram, Cluster
from diagrams.aws.compute import EC2
from diagrams.aws.network import VPC
from diagrams.aws.database import RDS
from diagrams.onprem.container import Docker

file_name = "diagram"
full_name = f"{file_name}"

with Diagram(full_name, show=True):
    with Cluster("Region: sa-east-1"):

        vpc = VPC("VPC")
        ec2_instance = EC2("EC2")
        ec2_instance_mongodb = EC2("EC2: MongoDB")
        database = RDS("RDS: Sql Server")
        docker_api = Docker("Api docker")

        # Connections
        vpc >> ec2_instance
        ec2_instance >> database
        ec2_instance >> ec2_instance_mongodb
        ec2_instance >> docker_api