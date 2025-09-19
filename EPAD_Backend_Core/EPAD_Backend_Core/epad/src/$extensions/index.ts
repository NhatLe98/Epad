const files = require.context('.', false, /^((?!index).)*.ts$/, 'sync');

files.keys().map(fileName => files(fileName));
export {};
