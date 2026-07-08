const http = require('http');

const options = {
  hostname: 'localhost',
  port: 5000,
  path: '/api/auth/login',
  method: 'POST',
  headers: {
    'Content-Type': 'application/json',
  }
};

const req = http.request(options, res => {
  let data = '';
  res.on('data', chunk => data += chunk);
  res.on('end', () => {
    const token = JSON.parse(data).data.token;
    
    const options2 = {
      hostname: 'localhost',
      port: 5000,
      path: '/api/admin/farms/1/observations/statistics/validation-summary',
      headers: {
        'Authorization': 'Bearer ' + token
      }
    };
    
    http.get(options2, res2 => {
      let data2 = '';
      res2.on('data', chunk => data2 += chunk);
      res2.on('end', () => {
        console.log(data2);
      });
    });
  });
});

req.write(JSON.stringify({email: 'admin@gmail.com', password: 'Password123!'}));
req.end();
