<template>
    <div>
        <el-input style="padding: 5px;"
                  :placeholder="$t('SearchData')"
                  suffix-icon="el-icon-search"
                  v-model="filterTree"
                  @keyup.enter.native="filterTreeData">
        </el-input>
        <el-tree :data="dataTree"
                 show-checkbox
                 ref="tree"
                 highlight-current
                 :props="{ label: 'Name', children: 'ListChildrent' }"
                 :filter-node-method="filterNode"
                 :default-expanded-keys="arrExpandedKeys"
                 :default-checked-keys="defaultChecked"
                 node-key="ID"
                 v-model="valTree"
                 v-loading="loadingTree">
            <template slot-scope="scoped">
                <div>
                    <i :class="getIconClass(scoped.data.Type)" />
                    <span class="ml-5">{{ scoped.data.Name }}</span>
                </div>
            </template>
        </el-tree>
    </div>
</template>
<script src="./department-tree-component.ts"></script>