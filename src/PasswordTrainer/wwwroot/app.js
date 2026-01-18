const form = document.getElementById("form");
const result = document.getElementById("result");

form.addEventListener("submit", async e => {
  e.preventDefault();
  result.textContent = "";

  const payload = {
    pin: document.getElementById("pin").value.trim(),
    id: document.getElementById("id").value.trim(),
    password: document.getElementById("pw").value.trim()
  };

  if (!payload.id || !payload.pin || !payload.password) {
    result.textContent = "⚠️ All fields are required";
    return;
  }

  try {
    const res = await fetch("/trainer/check", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(payload)
    });

    if (res.ok) {
      result.textContent = "✅ Correct";
      navigator.vibrate?.(100);
    } else {
      result.textContent = "❌ Incorrect";
    }
  } catch (err) {
    result.textContent = "⚠️ Connection error";
    console.error(err);
  } finally {
    document.getElementById("pw").value = "";
    document.getElementById("pw").focus();
  }
});
