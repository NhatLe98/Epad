import { departmentApi } from './department-api'

const Apis = {
  Department: departmentApi
}

export const ApiFactory = {
  get: name => Apis[name]
};

export default { ApiFactory };
