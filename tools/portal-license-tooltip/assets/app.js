document.addEventListener('DOMContentLoaded', function () {
  document.querySelectorAll('[data-password-visibility]').forEach(function (button) {
    button.addEventListener('click', function () {
      var field = document.getElementById(button.getAttribute('data-password-visibility'));
      if (!field) return;
      var visible = field.type === 'password';
      field.type = visible ? 'text' : 'password';
      button.textContent = visible ? 'Nascondi' : 'Mostra';
      button.setAttribute('aria-pressed', visible ? 'true' : 'false');
      field.focus();
    });
  });
  document.querySelectorAll('.data-table').forEach(function (table) {
    table.querySelectorAll('th[data-sort]').forEach(function (heading, index) {
      heading.setAttribute('role', 'button'); heading.setAttribute('tabindex', '0');
      function sortRows() {
        var body = table.tBodies[0]; if (!body) return;
        var ascending = heading.getAttribute('aria-sort') !== 'ascending';
        table.querySelectorAll('th[data-sort]').forEach(function (item) { item.removeAttribute('aria-sort'); });
        heading.setAttribute('aria-sort', ascending ? 'ascending' : 'descending');
        Array.prototype.slice.call(body.rows).sort(function (left, right) {
          var a = left.cells[index].textContent.trim(), b = right.cells[index].textContent.trim();
          if (heading.dataset.sort === 'date') { a = Date.parse(a) || 0; b = Date.parse(b) || 0; }
          else if (heading.dataset.sort === 'version') { a = a.split('.').map(Number); b = b.split('.').map(Number); for (var i=0;i<4;i++) { if ((a[i]||0)!==(b[i]||0)) return ((a[i]||0)-(b[i]||0))*(ascending?1:-1); } return 0; }
          else { a = a.toLocaleLowerCase(); b = b.toLocaleLowerCase(); }
          return (a < b ? -1 : a > b ? 1 : 0) * (ascending ? 1 : -1);
        }).forEach(function (row) { body.appendChild(row); });
      }
      heading.addEventListener('click', sortRows); heading.addEventListener('keydown', function (event) { if (event.key === 'Enter' || event.key === ' ') { event.preventDefault(); sortRows(); } });
    });
  });
  var forms = document.querySelectorAll('form');
  forms.forEach(function (form) {
    form.addEventListener('submit', function (event) {
      var confirmation = form.getAttribute('data-confirm');
      if (confirmation && !window.confirm(confirmation)) {
        event.preventDefault();
        return;
      }
      var button = form.querySelector('button[type="submit"]');
      if (button) button.setAttribute('aria-busy', 'true');
    });
  });

  var pushButton = document.querySelector('[data-push-toggle]');
  var originalTitle = document.title;
  var titleTimer = null;
  function api(action, options) {
    return fetch('index.php?action=' + action, Object.assign({credentials: 'same-origin'}, options || {})).then(function (response) {
      if (!response.ok) throw new Error('HTTP ' + response.status);
      return response.json();
    });
  }
  function base64Key(value) {
    var padding = '='.repeat((4 - value.length % 4) % 4);
    var raw = atob((value + padding).replace(/-/g, '+').replace(/_/g, '/'));
    return Uint8Array.from(raw, function (character) { return character.charCodeAt(0); });
  }
  function setPushState(state) {
    if (!pushButton) return;
    pushButton.hidden = false;
    pushButton.disabled = state === 'unsupported' || state === 'denied';
    pushButton.textContent = state === 'active' ? 'Notifiche attive' : state === 'denied' ? 'Notifiche bloccate' : state === 'unsupported' ? 'Notifiche non disponibili' : 'Attiva notifiche';
    pushButton.classList.toggle('push-active', state === 'active');
  }
  function flashTitle(count, preview) {
    var link = document.querySelector('.license-nav-link');
    if (link) {
      var marker = link.querySelector('.license-notification');
      if (count > 0 && !marker) {
        marker = document.createElement('span'); marker.className = 'license-notification'; marker.title = 'Nuove richieste o variazioni di licenza'; marker.innerHTML = '<span aria-hidden="true"></span>'; link.appendChild(marker);
      } else if (count === 0 && marker) marker.remove();
      link.title = count > 0 ? (preview || 'Nuove richieste o variazioni di licenza') : '';
      if (marker) {
        marker.title = link.title;
        marker.setAttribute('aria-label', count + ' notifiche. ' + link.title);
      }
    }
    if (count > 0 && document.hidden && !titleTimer) titleTimer = setInterval(function () { document.title = document.title === originalTitle ? 'NUOVA LICENZA SSW' : originalTitle; }, 900);
    if ((count === 0 || !document.hidden) && titleTimer) { clearInterval(titleTimer); titleTimer = null; document.title = originalTitle; }
  }
  function pollNotifications() { api('license_notification_count').then(function (data) { flashTitle(Number(data.count) || 0, data.preview); }).catch(function () {}); }
  if (pushButton && 'serviceWorker' in navigator && 'PushManager' in window && 'Notification' in window) {
    Promise.all([navigator.serviceWorker.register('service-worker.js'), api('push_config')]).then(function (values) {
      if (!values[1].enabled) return setPushState('unsupported');
      return values[0].pushManager.getSubscription().then(function (subscription) {
        setPushState(Notification.permission === 'denied' ? 'denied' : subscription ? 'active' : 'ready');
        pushButton.addEventListener('click', function () {
          if (subscription) return;
          Notification.requestPermission().then(function (permission) {
            if (permission !== 'granted') return setPushState(permission === 'denied' ? 'denied' : 'ready');
            return values[0].pushManager.subscribe({userVisibleOnly: true, applicationServerKey: base64Key(values[1].publicKey)});
          }).then(function (created) {
            if (!created) return;
            subscription = created;
            var body = new URLSearchParams();
            var json = created.toJSON();
            json.contentEncoding = (PushManager.supportedContentEncodings || ['aesgcm'])[0];
            body.set('csrf', pushButton.getAttribute('data-csrf')); body.set('subscription', JSON.stringify(json));
            return api('push_subscribe', {method: 'POST', headers: {'Content-Type': 'application/x-www-form-urlencoded;charset=UTF-8'}, body: body.toString()}).then(function () { setPushState('active'); });
          }).catch(function () { setPushState('ready'); });
        });
      });
    }).catch(function () { setPushState('unsupported'); });
  } else if (pushButton) setPushState('unsupported');
  document.addEventListener('visibilitychange', function () { if (!document.hidden) flashTitle(0); });
  if (document.querySelector('.license-nav-link')) { pollNotifications(); setInterval(pollNotifications, 25000); }
});
