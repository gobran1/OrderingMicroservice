  #!/bin/bash

  if ! curl -f http://localhost:${INVENTORY_PORT}/health/live; then
      echo "Liveness check failed"
      exit 1
  fi

  if ! curl -f http://localhost:${INVENTORY_PORT}/health/ready; then
      echo "Readiness check failed"
      exit 1
  fi

  echo "Health checks passed"
  exit 0
