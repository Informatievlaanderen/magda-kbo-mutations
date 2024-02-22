pushd src/AssociationRegistry.KboMutations.MutationLambda
./invoke.local.sh
popd

pushd src/AssociationRegistry.KboMutations.MutationFileLambda
./invoke.local.sh
popd

pushd src/AssociationRegistry.KboMutations.SyncLambda
./invoke.local.sh
popd
