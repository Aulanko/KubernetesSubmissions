(function () {
  const input = document.getElementById('todoInput');
  const btn = document.getElementById('sendBtn');
  const count = document.getElementById('charCount');
  const list = document.getElementById('todoList');
  const max = 140;
  const BACKEND = '/todo-backend';

  function update() {
    count.textContent = max - input.value.length;
    btn.disabled = input.value.trim().length === 0;
  }
  input.addEventListener('input', update);
  update();

  function renderTodos(todos) {
    list.innerHTML = '';
    todos
      .slice()
      .sort((a, b) => new Date(a.createdAt) - new Date(b.createdAt))
      .forEach(t => {
        const li = document.createElement('li');
        li.textContent = t.text;
        list.appendChild(li);
      });
  }

  async function loadTodos() {
    try {
      const res = await fetch(BACKEND + '/todos');
      if (!res.ok) throw new Error('failed to load todos');
      renderTodos(await res.json());
    } catch (err) {
      console.error(err);
    }
  }

  async function sendTodo() {
    const text = input.value.trim();
    if (!text) return;
    btn.disabled = true;
    try {
      const res = await fetch(BACKEND + '/todos', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ text })
      });
      if (!res.ok) {
        const err = await res.json().catch(() => ({}));
        alert(err.error || 'Failed to add todo');
        return;
      }
      input.value = '';
      update();
      await loadTodos();
    } catch (err) {
      console.error(err);
      alert('Network error while sending todo');
    } finally {
      btn.disabled = input.value.trim().length === 0;
    }
  }

  btn.addEventListener('click', e => { e.preventDefault(); sendTodo(); });
  input.addEventListener('keydown', e => { if (e.key === 'Enter') { e.preventDefault(); sendTodo(); } });

  loadTodos();
})();