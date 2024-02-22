pushd src/AssociationRegistry.KboMutations.MutationLambda
./deploy.local.sh
popd

pushd src/AssociationRegistry.KboMutations.MutationFileLambda
./deploy.local.sh
popd

pushd src/AssociationRegistry.KboMutations.SyncLambda
./deploy.local.sh
popd
