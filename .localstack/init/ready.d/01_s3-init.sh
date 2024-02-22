#!/bin/bash
awslocal s3api create-bucket --region us-east-1 --bucket verenigingsregister-kbomutations-mutationfile
awslocal s3api create-bucket --region us-east-1 --bucket verenigingsregister-kbomutations-sync


